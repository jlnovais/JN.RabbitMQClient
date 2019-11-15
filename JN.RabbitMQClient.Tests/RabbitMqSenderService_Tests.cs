using System;
using System.Collections.Generic;
using System.Text;
using JN.RabbitMQClient.Interfaces;
using JN.RabbitMQClient.Tests.HelperClasses;
using NUnit.Framework;

namespace JN.RabbitMQClient.Tests
{
    class RabbitMqSenderService_Tests
    {
        private const string queueName = "TestQueue";


        private readonly RabbitMqHelper _rabbitMqHelper = new RabbitMqHelper(new BrokerConfig()
        {
            HostName = "localhost",
            Password = "123",
            UserName = "test",
            VirtualHost = "/"
        });

        private readonly IBrokerConfigSender _brokerConfig = new Entities.BrokerConfig()
        {
            Username = "test",
            Password = "123",
            Host = "localhost",
            RoutingKeyOrQueueName = queueName,
            VirtualHost = "/",
        };


        [SetUp]
        public void Setup()
        {
            _rabbitMqHelper.DeleteQueue(queueName);
            _rabbitMqHelper.CreateQueueOrGetInfo(queueName);
        }

        [TearDown]
        public void TearDown()
        {
            _rabbitMqHelper.DeleteQueue(queueName);
        }

        private IRabbitMqSenderService GetSenderService(IBrokerConfigSender config = null)
        {
            var consumer = new RabbitMqSenderService(config ?? _brokerConfig)
            {
                ServiceDescription = "Test service - sender"
            };

            return consumer;
        }


        [Test]
        public void SenderService_Send_messageIsInTheQueue()
        {
            var senderService = GetSenderService();

            senderService.Send("message", "", queueName, Encoding.UTF8, false);

            var res = _rabbitMqHelper.CreateQueueOrGetInfo(queueName);
            var totalMessagesInQueue = res.MessageCount;

            Assert.AreEqual(1, totalMessagesInQueue);
        }

        [Test]
        public void SenderService_Send_CreateQueue_messageIsInTheQueue()
        {
            //queue was created in SetUp - delete here
            _rabbitMqHelper.DeleteQueue(queueName);

            var senderService = GetSenderService();

            senderService.Send("message", "", queueName, Encoding.UTF8, true);

            var res = _rabbitMqHelper.CreateQueueOrGetInfo(queueName);
            var totalMessagesInQueue = res.MessageCount;

            Assert.AreEqual(1, totalMessagesInQueue);
        }

        [Test]
        public void SenderService_Send_CreateQueue_ExistingQueue_messageIsInTheQueue()
        {
            //queue was created in SetUp - will cause error
            var senderService = GetSenderService();

            senderService.Send("message", "", queueName, Encoding.UTF8, true);

            var res = _rabbitMqHelper.CreateQueueOrGetInfo(queueName);
            var totalMessagesInQueue = res.MessageCount;

            Assert.AreEqual(1, totalMessagesInQueue);
        }
    }
}
