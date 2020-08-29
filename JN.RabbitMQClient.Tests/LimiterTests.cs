using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JN.RabbitMQClient.Limiter;
using NUnit.Framework;

namespace JN.RabbitMQClient.Tests
{

    public class LimiterAux : WindowLimiter
    {
        public LimiterAux(int maxAllowed, int windowSeconds) : base(maxAllowed, windowSeconds, Constants.MessageProcessInstruction.OK, Constants.MessageProcessInstruction.RequeueMessageWithDelay)
        {
        }

        public bool IsAllowedAux(DateTime date)
        {
            return base.IsAllowed(date);
        }
    }

    public class LimiterTests
    {

        private const int maxAllowedGlobal = 2;
        private const int windowSecondsGlobal = 1;
        private LimiterAux limiterGlobal;

        private int maxExecuted;

        private void Execute()
        {
            for (var i = 0; i < 100; i++)
            {
                limiterGlobal.IsAllowed();

                if (maxExecuted < limiterGlobal.WindowCount)
                {
                    maxExecuted = limiterGlobal.WindowCount;
                }
            }
        }


        [Test]
        public async Task IsAllowed_MultiThread_DoesNotExceedMax()
        {
            limiterGlobal = new LimiterAux(maxAllowedGlobal, windowSecondsGlobal);

            var tasks = new List<Task>
            {
                Task.Run(Execute), Task.Run(Execute), Task.Run(Execute), Task.Run(Execute)
            };

            await Task.WhenAll(tasks).ConfigureAwait(false);

            Assert.AreEqual(maxAllowedGlobal, maxExecuted);
        }


        [TestCase(0, 1)]
        [TestCase(1, 0)]
        [TestCase(0, 0)]
        public void New_InvalidParameters_ThrowsException(int max, int windowSeconds)
        {
            Assert.Throws<ArgumentException>(() => new LimiterAux(max, windowSeconds));
        }

        [Test]
        public void IsAllowed_DoesNotExceedMax()
        {
            const int maxAllowed = 2;
            const int windowSeconds = 1;

            var limiter = new LimiterAux(maxAllowed, windowSeconds);

            var totalExecuted = Convert.ToInt32(limiter.IsAllowed());
            totalExecuted += Convert.ToInt32(limiter.IsAllowed());
            totalExecuted += Convert.ToInt32(limiter.IsAllowed());
            totalExecuted += Convert.ToInt32(limiter.IsAllowed());
            totalExecuted += Convert.ToInt32(limiter.IsAllowed());

            Assert.AreEqual(maxAllowed, totalExecuted);
        }

        [Test]
        public void IsAllowed_NewWindow_CounterIsReset()
        {
            var totalExecuted = 0;
            const int maxAllowed = 2;
            const int windowSeconds = 1;

            var limiter = new LimiterAux(maxAllowed, windowSeconds);

            totalExecuted = Convert.ToInt32(limiter.IsAllowed());
            totalExecuted += Convert.ToInt32(limiter.IsAllowed());
            totalExecuted += Convert.ToInt32(limiter.IsAllowed());
            totalExecuted += Convert.ToInt32(limiter.IsAllowed());
            totalExecuted += Convert.ToInt32(limiter.IsAllowed());

            Assert.AreEqual(maxAllowed, totalExecuted);

            totalExecuted = Convert.ToInt32(limiter.IsAllowedAux(DateTime.Now.AddSeconds(10)));

            Assert.AreEqual(1, totalExecuted);
        }

        [Test]
        public void IsAllowed_WindowCount_IsCorrect()
        {
            const int maxAllowed = 2;
            const int windowSeconds = 1;

            var limiter = new LimiterAux(maxAllowed, windowSeconds);

            var totalExecuted = Convert.ToInt32(limiter.IsAllowed());
            totalExecuted += Convert.ToInt32(limiter.IsAllowed());
            totalExecuted += Convert.ToInt32(limiter.IsAllowed());
            totalExecuted += Convert.ToInt32(limiter.IsAllowed());
            totalExecuted += Convert.ToInt32(limiter.IsAllowed());

            Assert.AreEqual(totalExecuted, limiter.WindowCount, $"totalExecuted: {totalExecuted} | limiter.WindowCount: {limiter.WindowCount}");
        }


        [Test]
        public void IsAllowed_InvalidDate_ReturnsFalse()
        {
            const int maxAllowed = 2;
            const int windowSeconds = 1;

            var limiter = new LimiterAux(maxAllowed, windowSeconds);

            var totalExecuted = Convert.ToInt32(limiter.IsAllowed()); Console.WriteLine(limiter.WindowCount);

            Assert.AreEqual(1, totalExecuted);
            Assert.AreEqual(1, limiter.WindowCount);

            var oldWindowCount = limiter.WindowCount;

            totalExecuted = Convert.ToInt32(limiter.IsAllowedAux(DateTime.Now.AddSeconds(-10))); Console.WriteLine(limiter.WindowCount);
            totalExecuted += Convert.ToInt32(limiter.IsAllowedAux(DateTime.Now.AddSeconds(-10))); Console.WriteLine(limiter.WindowCount);


            Assert.AreEqual(0, totalExecuted);
            Assert.AreEqual(oldWindowCount, limiter.WindowCount);
        }

    }
}