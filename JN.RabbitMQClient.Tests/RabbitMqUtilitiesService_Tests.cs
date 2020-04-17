using JN.RabbitMQClient.Interfaces;
using JN.RabbitMQClient.Tests.HelperClasses;
using NUnit.Framework;

namespace JN.RabbitMQClient.Tests
{
    class RabbitMqUtilitiesService_Tests
    {
        private const string queueName = "TestQueue_created";
        private const string exchangeName = "TestExchange_created";

        private readonly IBrokerConfig _brokerConfig = new Entities.BrokerConfig
        {
            Username = "test",
            Password = "123",
            Host = "localhost",
            VirtualHost = "/",
        };

        private readonly IBrokerConfig _brokerConfigInvalidHost = new Entities.BrokerConfig
        {
            Username = "test",
            Password = "123",
            Host = "invalid",
            VirtualHost = "/",
        };


        private readonly IBrokerConfigSender _brokerConfigSender = new Entities.BrokerConfigSender
        {
            Username = "test",
            Password = "123",
            Host = "localhost",
            RoutingKeyOrQueueName = queueName,
            VirtualHost = "/",
        };


        private IRabbitMqUtilitiesService GetService(IBrokerConfig config = null)
        {
            var service = new RabbitMqUtilitiesService(config ?? _brokerConfig)
            {
                ServiceDescription = "Test service - sender"
            };

            return service;
        }

        private IRabbitMqSenderService GetSenderService(IBrokerConfigSender config = null)
        {
            var service = new RabbitMqSenderService(config ?? _brokerConfigSender)
            {
                ServiceDescription = "Test service - sender"
            };

            return service;
        }



        private readonly RabbitMqHelper _rabbitMqHelper = new RabbitMqHelper(new BrokerConfig
        {
            HostName = "localhost",
            Password = "123",
            UserName = "test",
            VirtualHost = "/"
        });




        [SetUp]
        public void Setup()
        {
            CleanAll();

            //_rabbitMqHelper.CreateQueueOrGetInfo(queueName);
            _rabbitMqHelper.CreateExchangeAndBindQueue(exchangeName, null);
        }

        [TearDown]
        public void TearDown()
        {
            CleanAll();
        }

        private void CleanAll()
        {
            _rabbitMqHelper.DeleteQueue(queueName);
            _rabbitMqHelper.DeleteExchangeAndBindedQueues(exchangeName, null);
        }


        [Test]
        public void SenderService_CreateQueue_returnsOk()
        {
            var service = GetService();
            var sender = GetSenderService();

            var result = service.CreateQueue(queueName);
            sender.Send("message");

            var totalMessagesInQueue = _rabbitMqHelper.GetTotalItems(queueName);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, totalMessagesInQueue);
        }

        [Test]
        public void SenderService_CreateQueue_invalidHost_returnsError()
        {
            var service = GetService(_brokerConfigInvalidHost);

            var result = service.CreateQueue(queueName);

            Assert.IsFalse(result.Success);
            Assert.That(result.ErrorCode < 0);
        }


        [Test]
        public void SenderService_CreateQueue_BindToExchange()
        {
            var service = GetService();
            var sender = GetSenderService();

            var res = service.CreateQueue(queueName, exchangeName, "key");
            sender.Send("message", exchangeName, "key");

            var totalMessagesInQueue = _rabbitMqHelper.GetTotalItems(queueName);

            Assert.IsTrue(res.Success);
            Assert.AreEqual(1, totalMessagesInQueue);
        }

        [Test]
        public void SenderService_CreateQueue_BindToNonExistingExchange_returnsError()
        {
            var service = GetService();

            var result = service.CreateQueue(queueName, "nonExistingExchangeName", "key");

            Assert.IsFalse(result.Success);
            Assert.That(result.ErrorCode < 0);
        }



        [Test]
        public void SenderService_GetTotalMessages_ExistingQueue_returnsExpectedValue()
        {
            var service = GetService();
            var sender = GetSenderService();

            sender.Send("message", "", queueName, true);

            var res = service.GetTotalMessages(queueName);

            Assert.IsTrue(res.Success);
            Assert.AreEqual(1, res.ReturnedObject);
        }


        [Test]
        public void SenderService_GetTotalMessages_InvalidHost_returnsError()
        {
            var service = GetService(_brokerConfigInvalidHost);

            var result = service.GetTotalMessages(queueName);

            Assert.IsFalse(result.Success);
            Assert.That(result.ErrorCode < 0);
        }

        [Test]
        public void SenderService_GetTotalMessages_NonExistingQueue_returnsError()
        {
            var service = GetService();

            var result = service.GetTotalMessages(queueName);

            Assert.IsFalse(result.Success);
            Assert.That(result.ErrorCode < 0);
        }


        [Test]
        public void SenderService_Delete_ExistingQueue_returnsOk()
        {
            var service = GetService();
            var sender = GetSenderService();

            service.CreateQueue(queueName);
            sender.Send("message");

            var totalMessagesInQueue = _rabbitMqHelper.GetTotalItems(queueName);

            var result = service.DeleteQueue(queueName);

            Assert.AreEqual(1, totalMessagesInQueue);
            Assert.IsTrue(result.Success);
        }

        [Test]
        public void SenderService_Delete_NonExistingQueue_returnsOk()
        {
            var service = GetService();

            var res = service.DeleteQueue(queueName);

            Assert.IsTrue(res.Success);
        }


        [Test]
        public void SenderService_Delete_InvalidHost_returnsError()
        {
            var service = GetService(_brokerConfigInvalidHost);

            var result = service.DeleteQueue(queueName);

            Assert.IsFalse(result.Success);
            Assert.That(result.ErrorCode < 0);
        }

    }
}
