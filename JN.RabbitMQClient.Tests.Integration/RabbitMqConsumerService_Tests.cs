using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JN.RabbitMQClient.Entities;
using JN.RabbitMQClient.Interfaces;
using JN.RabbitMQClient.Limiter;
using JN.RabbitMQClient.Tests.HelperClasses;
using JN.RabbitMQClient.Tests.Integration.HelperClasses;
using NUnit.Framework;
using BrokerConfig = JN.RabbitMQClient.Tests.HelperClasses.BrokerConfig;


namespace JN.RabbitMQClient.Tests.Integration
{
    public class RabbitMqConsumerService_Tests
    {
        private int _totalMessagesReceived;
        private int _totalStopProcessing;
        private int _totalErrors;

        private const bool useTLS = false;
        private const string host = "localhost";

        private const int TotalConsumers = 2;
        private const int DefaultTotalConsumers = 5;
        private const string queueName = "TestQueue";
        private const string queueNameRetry = "TestQueueRetry";

        private const string otherQueueName = "TestOtherQueue";

        private readonly RabbitMqHelper _rabbitMqHelper = new RabbitMqHelper(new BrokerConfig
        {
            HostName = host,
            Password = "123",
            UserName = "test",
            VirtualHost = "/",

        });

        private readonly IBrokerConfigConsumers _brokerConfig = new Entities.BrokerConfigConsumers
        {
            Username = "test",
            Password = "123",
            Host = host,
            RoutingKeyOrQueueName = queueName,
            VirtualHost = "/",
            TotalInstances = DefaultTotalConsumers,
            UseTLS =  useTLS
            //Port = 9999
        };

        private IRabbitMqConsumerService _consumerService;

        private IRabbitMqConsumerService GetConsumerService(IBrokerConfigConsumers config = null)
        {

            var service = new RabbitMqConsumerService(config ?? _brokerConfig)
            {
                ServiceDescription = "Test service"
            };

            service.ReceiveMessage += ReceiveMessage;
            service.ReceiveMessageError += ReceiveMessageError;
            service.ShutdownConsumer += ShutDownConsumer;

            return service;
        }



        private Task ShutDownConsumer(string consumertag, ushort errorcode, string shutdowninitiator, string errormessage)
        {
            Interlocked.Increment(ref _totalStopProcessing);
            return Task.CompletedTask;
        }

        private Task ReceiveMessageError(string routingkeyorqueuename, string consumertag, string exchange,
            string message, string errormessage)
        {
            _totalErrors++;
            return Task.CompletedTask;
        }

        private Task<MessageProcessInstruction> ReceiveMessage(string routingkeyorqueuename, string consumertag, long firsterrortimestamp, string exchange, string message, string additionalInfo)
        {
            _totalMessagesReceived++;

            switch (message)
            {
                case "error":
                    throw new RabbitMqClientException("error");
                case "ok":
                    return Task.FromResult(new MessageProcessInstruction(Constants.MessageProcessInstruction.OK));
                case "ignore":
                    return Task.FromResult(new MessageProcessInstruction(Constants.MessageProcessInstruction.IgnoreMessage));
                case "ignoreRequeue":
                    return Task.FromResult(new MessageProcessInstruction(Constants.MessageProcessInstruction.IgnoreMessageWithRequeue));
                case "RequeueDelay":
                    return Task.FromResult(new MessageProcessInstruction(Constants.MessageProcessInstruction.RequeueMessageWithDelay, "message delay"));
                default:
                    return Task.FromResult(new MessageProcessInstruction(Constants.MessageProcessInstruction.Unknown));
            }
        }


        [SetUp]
        public void Setup()
        {
            _rabbitMqHelper.DeleteQueue(queueName);
            _rabbitMqHelper.DeleteQueue(otherQueueName);
            _rabbitMqHelper.DeleteQueue(queueNameRetry);

            _rabbitMqHelper.CreateQueueOrGetInfo(queueName);
            _rabbitMqHelper.CreateQueueOrGetInfo(queueNameRetry);

            _totalMessagesReceived = 0;
            _totalStopProcessing = 0;
        }

        [TearDown]
        public void TearDown()
        {
            _rabbitMqHelper.DeleteQueue(queueName);
            _rabbitMqHelper.DeleteQueue(otherQueueName);
            _rabbitMqHelper.DeleteQueue(queueNameRetry);
        }

        [TestCase("")]
        [TestCase(";")]
        [TestCase("  ")]
        [TestCase(null)]
        public void ConsumerService_StartConsumers_InvalidHost_ThrowsException(string invalidHost)
        {

            IBrokerConfigConsumers config = new Entities.BrokerConfigConsumers
            {
                Host = invalidHost,
            };

            _consumerService = GetConsumerService(config);

            Assert.Throws<ArgumentException>(() => _consumerService.StartConsumers("test", null, TotalConsumers));
        }

        [Test]
        public void ConsumerService_StartConsumers_ValidNumberOfConsumers_returnsOk()
        {
            _consumerService = GetConsumerService();

            _consumerService.StartConsumers("test", null, TotalConsumers);

            Thread.Sleep(100);

            var startedConsumers = _consumerService.GetTotalRunningConsumers;
            var totalConsumers = _consumerService.GetTotalConsumers;

            var res = _rabbitMqHelper.CreateQueueOrGetInfo(queueName);
            var consumerCount = res.ConsumerCount;

            Assert.AreEqual(TotalConsumers, startedConsumers);
            Assert.AreEqual(TotalConsumers, totalConsumers);
            Assert.AreEqual(TotalConsumers, consumerCount);
        }

        [Test]
        public void ConsumerService_StartConsumers_NamesAreCorrect()
        {
            _consumerService = GetConsumerService();

            _consumerService.StartConsumers("test", null, 2);

            Thread.Sleep(100);

            var startedConsumers = _consumerService.GetConsumerDetails();

            Assert.IsTrue(startedConsumers.Any(y => y.Name == "test_0"));
            Assert.IsTrue(startedConsumers.Any(y => y.Name == "test_1"));

        }



        [Test]
        public void ConsumerService_StartConsumers_ValidNumberOfConsumers_useDefaultConsumers_returnsOk()
        {
            _consumerService = GetConsumerService();

            _consumerService.StartConsumers("test");

            Thread.Sleep(300);

            var startedConsumers = _consumerService.GetTotalRunningConsumers;
            var totalConsumers = _consumerService.GetTotalConsumers;

            var res = _rabbitMqHelper.CreateQueueOrGetInfo(queueName);
            var consumerCount = res.ConsumerCount;

            Assert.AreEqual(DefaultTotalConsumers, startedConsumers);
            Assert.AreEqual(DefaultTotalConsumers, totalConsumers);
            Assert.AreEqual(DefaultTotalConsumers, consumerCount);
        }




        [Test]
        public void ConsumerService_StartConsumers_InValidNumberOfConsumers_ThrowsException()
        {
            _consumerService = GetConsumerService();

            Assert.Throws<ArgumentException>(() => _consumerService.StartConsumers("test", null, 0));
        }

        [Test]
        public void ConsumerService_ReceiveMessage_receivesAllMessages()
        {
            _consumerService = GetConsumerService();

            _rabbitMqHelper.SendMessage(queueName, "message 1");
            _rabbitMqHelper.SendMessage(queueName, "message 2");

            _consumerService.StartConsumers("test", null, TotalConsumers);

            Thread.Sleep(100);

            var res = _rabbitMqHelper.CreateQueueOrGetInfo(queueName);
            var totalMessagesInQueue = res.MessageCount;

            Assert.AreEqual(2, _totalMessagesReceived);
            Assert.AreEqual(0, totalMessagesInQueue);

        }

        [Test]
        public void ConsumerService_ReceiveMessage_CreateQueue_queueAlreadyExists_receivesAllMessages()
        {
            _consumerService = GetConsumerService();

            _rabbitMqHelper.SendMessage(queueName, "message 1");
            _rabbitMqHelper.SendMessage(queueName, "message 2");

            _consumerService.StartConsumers("test", null, TotalConsumers, true);

            Thread.Sleep(100);

            var res = _rabbitMqHelper.CreateQueueOrGetInfo(queueName);
            var totalMessagesInQueue = res.MessageCount;

            Assert.AreEqual(2, _totalMessagesReceived);
            Assert.AreEqual(0, totalMessagesInQueue);

        }

        [Test]
        public void ConsumerService_ReceiveMessage_CreateQueue_queueDoesnt_existYet_receivesAllMessages()
        {
            _consumerService = GetConsumerService();

            _consumerService.StartConsumers("test", otherQueueName, TotalConsumers, true);

            _rabbitMqHelper.SendMessage(otherQueueName, "message 1");
            _rabbitMqHelper.SendMessage(otherQueueName, "message 2");

            Thread.Sleep(100);

            var res = _rabbitMqHelper.CreateQueueOrGetInfo(otherQueueName);
            var totalMessagesInQueue = res.MessageCount;

            Assert.AreEqual(2, _totalMessagesReceived);
            Assert.AreEqual(0, totalMessagesInQueue);

        }

        [Test]
        public void ConsumerService_StopConsumers_executesAllDisposeDelegates()
        {
            _consumerService = GetConsumerService();

            _consumerService.StartConsumers("test", null, TotalConsumers);

            _rabbitMqHelper.SendMessage(queueName, "message 1");
            _rabbitMqHelper.SendMessage(queueName, "message 2");

            Thread.Sleep(100);

            _consumerService.StopConsumers();

            Thread.Sleep(100);

            Assert.AreEqual(TotalConsumers, _totalStopProcessing, $"_totalStopProcessing: {_totalStopProcessing}");
        }

        [Test]
        public void ConsumerService_Dispose_executesAllDisposeDelegates()
        {
            _consumerService = GetConsumerService();

            _consumerService.StartConsumers("test", null, TotalConsumers);

            _rabbitMqHelper.SendMessage(queueName, "message 1");
            _rabbitMqHelper.SendMessage(queueName, "message 2");

            Thread.Sleep(100);

            _consumerService.Dispose();

            Thread.Sleep(2500);

            Assert.AreEqual(TotalConsumers, _totalStopProcessing, $"_totalStopProcessing: {_totalStopProcessing}");
        }


        [Test]
        public void ConsumerService_ProcessErrors_executesAllErrorDelegates()
        {
            _consumerService = GetConsumerService();

            _consumerService.StartConsumers("test", null, TotalConsumers);

            _rabbitMqHelper.SendMessage(queueName, "error");
            _rabbitMqHelper.SendMessage(queueName, "error");

            Thread.Sleep(100);

            Assert.AreEqual(TotalConsumers, _totalErrors);
        }

        [Test]
        public void ConsumerService_ProcessMessage_WithLimiter_limiterIsCalled()
        {
            const string messageType = "ok";
            const int expectedCalls = 1;

            var limiter = new LimiterHelper(Constants.MessageProcessInstruction.OK);
            
            _consumerService = GetConsumerService();

            _consumerService.Limiter = limiter;

            _rabbitMqHelper.SendMessage(queueName, messageType);

            _consumerService.StartConsumers("test", null, TotalConsumers);

            Thread.Sleep(100);

            _consumerService.Dispose();

            Assert.AreEqual(expectedCalls, limiter.TotalCalls);

        }


        [TestCase("ok", 0)]
        [TestCase("ignore", 0)]
        [TestCase("ignoreRequeue", 1)]
        [TestCase("RequeueDelay", 0)]
        [TestCase("unknownState", 0)]
        public void ConsumerService_ProcessMessage_messageIsReceived(string messageType,
            int expectedMessageInQueueAfterProcessing)
        {
            _consumerService = GetConsumerService();

            _rabbitMqHelper.SendMessage(queueName, messageType);

            _consumerService.StartConsumers("test", null, TotalConsumers);

            Thread.Sleep(100);

            _consumerService.Dispose();

            var res = _rabbitMqHelper.CreateQueueOrGetInfo(queueName);
            var totalMessagesInQueue = res.MessageCount;

            Assert.AreEqual(expectedMessageInQueueAfterProcessing, totalMessagesInQueue);

        }

        [TestCase("ok", 0, 0)]
        [TestCase("ignore", 0, 0)]
        [TestCase("ignoreRequeue", 1, 0)]
        [TestCase("RequeueDelay", 0, 1)]
        [TestCase("unknownState", 0, 0)]
        public void ConsumerService_ProcessMessageWithDelay_messageIsReceived(string messageType,
            int expectedMessageInQueueAfterProcessing, int expectedMessageInRetryQueueAfterProcessing)
        {
            _consumerService = GetConsumerService();

            _rabbitMqHelper.SendMessage(queueName, messageType);

            _consumerService.StartConsumers("test", GetRetryOptions(), null, TotalConsumers);

            Thread.Sleep(100);

            _consumerService.Dispose();

            var resQueue = _rabbitMqHelper.CreateQueueOrGetInfo(queueName);
            var resRetryQueue = _rabbitMqHelper.CreateQueueOrGetInfo(queueNameRetry);

            var totalMessagesInQueue = resQueue.MessageCount;
            var totalMessagesInRetryQueue = resRetryQueue.MessageCount;

            Assert.AreEqual(expectedMessageInQueueAfterProcessing, totalMessagesInQueue);
            Assert.AreEqual(expectedMessageInRetryQueueAfterProcessing, totalMessagesInRetryQueue);

        }

        private static RetryQueueDetails GetRetryOptions()
        {
            return new RetryQueueDetails
            {
                RetentionPeriodInRetryQueueMilliseconds = 60000,
                RetryQueue = queueNameRetry
            };
        }


        [Test]
        public void ConsumerService_StopConsumers_AllConsumersStopConsuming()
        {
            _consumerService = GetConsumerService();

            _consumerService.StartConsumers("test", null, TotalConsumers);
 
            Thread.Sleep(100);

            Assert.AreEqual(TotalConsumers, _consumerService.GetTotalRunningConsumers);

            Thread.Sleep(100);

            _consumerService.StopConsumers();

            Thread.Sleep(100);

            var running = _consumerService.GetTotalRunningConsumers;
            var total = _consumerService.GetTotalConsumers;

            Assert.AreEqual(0, running);
            Assert.AreEqual(TotalConsumers, total);
        }

        [Test]
        public void ConsumerService_StopConsumers_oneConsumer_consumerStopConsuming()
        {
            _consumerService = GetConsumerService();

            _consumerService.StartConsumers("test", null, TotalConsumers);

            Thread.Sleep(100);

            Assert.AreEqual(TotalConsumers, _consumerService.GetTotalRunningConsumers);

            _consumerService.StopConsumers("test_0");

            Thread.Sleep(200);

            Assert.AreEqual(1, _consumerService.GetTotalRunningConsumers);
            Assert.AreEqual(TotalConsumers, _consumerService.GetTotalConsumers);
        }

    }
}
