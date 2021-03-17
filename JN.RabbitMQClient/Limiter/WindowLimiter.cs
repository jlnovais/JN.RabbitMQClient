using System;

namespace JN.RabbitMQClient.Limiter
{
    /// <summary>
    /// Limits the number of items to be processed
    /// </summary>
    public class WindowLimiter : IWindowLimiter
    {
        public DateTime Start { get; private set; }
        public DateTime End { get; private set; }

        public int MaxAllowed { get; }
        public int WindowCount { get; private set; }
        public int WindowSeconds { get; }

        private readonly object _lockObj = new object();

        /// <summary>
        /// Limits the number of items to be processed. It allows processing 'maxAllowed' items every 'windowSeconds' seconds and if that
        /// limit is exceeded then the processing instruction that should be considered is 'deniedProcessInstruction' 
        /// </summary>
        /// <param name="maxAllowed">max items allowed</param>
        /// <param name="windowSeconds"></param>
        /// <param name="deniedProcessInstruction"></param>
        public WindowLimiter(int maxAllowed, int windowSeconds, 
            Constants.MessageProcessInstruction deniedProcessInstruction)
        {
            if (maxAllowed <= 0 || windowSeconds <= 0)
            {
                throw new ArgumentException("Invalid parameters; cannot be 0");
            }

            MaxAllowed = maxAllowed;
            WindowSeconds = windowSeconds;
            DeniedProcessInstruction = deniedProcessInstruction;

            Start = DateTime.Now;
            End = Start.AddSeconds(windowSeconds);
        }

        public Constants.MessageProcessInstruction DeniedProcessInstruction { get; }

        public bool IsAllowed(string routingKeyOrQueueName, string consumerTag, long firstErrorTimestamp, string exchange,
            string message)
        {
            return IsAllowed(DateTime.Now);
        }

        protected bool IsAllowed(DateTime processingTime)
        {
            lock (_lockObj)
            {
                if (processingTime < Start)
                {
                    return false;
                }

                if (processingTime > End)
                {
                    Start = processingTime;
                    End = Start.AddSeconds(WindowSeconds);
                    WindowCount = 0;
                }

                var result = WindowCount < MaxAllowed;

                if (result)
                {
                    WindowCount++;
                }

                return result;
            }

        }


    }
}
