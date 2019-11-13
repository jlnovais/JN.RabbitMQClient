using System;
using System.Threading;
using System.Threading.Tasks;
using JN.RabbitMQClient.Interfaces;
using JN.RabbitMQClient.Tests.HelperClasses;
using NUnit.Framework;


//[assembly: IgnoresAccessChecksTo("RabbitMQToolsV2.Consumer")]
namespace JN.RabbitMQClient.Tests
{


    class RabbitMqConsumerService_Tests
    {
        private static int _totalMessagesReceived;
        private static int _totalStopProcessing;
        private static int _totalErrors;

        private const int TotalConsumers = 2;
        private const string queueName = "TestQueue";

        private readonly RabbitMqHelper _rabbitMqHelper = new RabbitMqHelper(new BrokerConfig()
        {
            HostName = "localhost",
            Password = "123",
            UserName = "test",
            VirtualHost = "/"
        });

        private readonly IBrokerConfigConsumers _brokerConfig = new Entities.BrokerConfig()
        {
            Username = "test",
            Password = "123",
            Host = "localhost",
            RoutingKeyOrQueueName = queueName,
            VirtualHost = "/",
        };

        private IRabbitMqConsumerService _consumerService;

        private IRabbitMqConsumerService GetConsumerService(IBrokerConfigConsumers config = null)
        {

            var consumer = new RabbitMqConsumerService(config ?? _brokerConfig)
            {
                ServiceDescription = "Test service"
            };

            consumer.ReceiveMessage += ReceiveMessage;
            consumer.ReceiveMessageError += ReceiveMessageError;
            consumer.ShutdownConsumer += ShutDownConsumer;

            return consumer;
        }

        private Task ShutDownConsumer(string consumertag, ushort errorcode, string shutdowninitiator, string errormessage)
        {
            _totalStopProcessing++;
            return Task.CompletedTask;
        }

        private Task ReceiveMessageError(string routingkeyorqueuename, string consumertag, string exchange,
            string message, string errormessage)
        {
            _totalErrors++;
            return Task.CompletedTask;
        }

        private Task<Constants.MessageProcessInstruction> ReceiveMessage(string routingkeyorqueuename, string consumertag, string exchange, string message)
        {
            _totalMessagesReceived++;

            switch (message)
            {
                case "error":
                    throw new Exception("error");
                case "ignore":
                    return Task.FromResult(Constants.MessageProcessInstruction.IgnoreMessage);
                default:
                    return Task.FromResult(Constants.MessageProcessInstruction.OK);
            }
        }


        [SetUp]
        public void Setup()
        {
            _rabbitMqHelper.CreateQueueOrGetInfo(queueName);

            _totalMessagesReceived = 0;
            _totalStopProcessing = 0;
        }

        [TearDown]
        public void TearDown()
        {
            _rabbitMqHelper.DeleteQueue(queueName);
        }

        [TestCase("" )]
        [TestCase(";")]
        [TestCase("  ")]
        [TestCase(null)]
        public void ConsumerService_StartConsumers_InvalidHost_ThrowsException(string invalidHost)
        {

            IBrokerConfigConsumers config = new Entities.BrokerConfig()
            {
                Host = invalidHost,
            };

            _consumerService = GetConsumerService(config);

            Assert.Throws<Exception>(() => _consumerService.StartConsumers("test", null, TotalConsumers));
        }

        [Test]
        public void ConsumerService_StartConsumers_ValidNumberOfConsumers_returnsOk()
        {
            _consumerService = GetConsumerService();

            _consumerService.StartConsumers("test", null, TotalConsumers);

            Thread.Sleep(100);

            var startedConsumers = _consumerService.GetTotalRunningConsumers;

            var res = _rabbitMqHelper.CreateQueueOrGetInfo(queueName);
            var consumerCount = res.ConsumerCount;

            Assert.AreEqual(TotalConsumers, startedConsumers);
            Assert.AreEqual(TotalConsumers, consumerCount);
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
        public void ConsumerService_Dispose_executesAllDisposeDelegates()
        {
            _consumerService = GetConsumerService();

            _consumerService.StartConsumers("test", null, TotalConsumers);

            Thread.Sleep(100);
            
            _consumerService.Dispose();

            Thread.Sleep(100);

            Assert.AreEqual(2, _totalStopProcessing);
        }

        [Test]
        public void ConsumerService_ProcessErrors_executesAllErrorDelegates()
        {
            _consumerService = GetConsumerService();

            _consumerService.StartConsumers("test", null, TotalConsumers);

            _rabbitMqHelper.SendMessage(queueName, "error");
            _rabbitMqHelper.SendMessage(queueName, "error");

            Thread.Sleep(100);

            Assert.AreEqual(2, _totalErrors);
        }

    }
}
