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

        private const string exchangeName = "TestExchange";

        private const string queueNameExchange1 = "TestQueueEx1";
        private const string queueNameExchange2 = "TestQueueEx2";


        private readonly RabbitMqHelper _rabbitMqHelper = new RabbitMqHelper(new BrokerConfig()
        {
            HostName = "localhost",
            Password = "123",
            UserName = "test",
            VirtualHost = "/"
        });

        private readonly IBrokerConfigSender _brokerConfig = new Entities.BrokerConfigSender
        {
            Username = "test",
            Password = "123",
            Host = "localhost",
            RoutingKeyOrQueueName = queueName,
            VirtualHost = "/",
            //Exchange = ""
        };


        [SetUp]
        public void Setup()
        {
            CleanAll();

            _rabbitMqHelper.CreateQueueOrGetInfo(queueName);
            _rabbitMqHelper.CreateQueueOrGetInfo(queueNameExchange1);
            _rabbitMqHelper.CreateQueueOrGetInfo(queueNameExchange2);
            _rabbitMqHelper.CreateExchangeAndBindQueue(exchangeName,
                new List<string> { queueNameExchange1, queueNameExchange2 });
        }

        private void CleanAll()
        {
            _rabbitMqHelper.DeleteQueue(queueName);
            _rabbitMqHelper.DeleteQueue(queueNameExchange1);
            _rabbitMqHelper.DeleteQueue(queueNameExchange2);
            _rabbitMqHelper.DeleteExchangeAndBindedQueues(exchangeName, null);
        }

        [TearDown]
        public void TearDown()
        {
            CleanAll();
        }

        private IRabbitMqSenderService GetSenderService(IBrokerConfigSender config = null)
        {
            var service = new RabbitMqSenderService(config ?? _brokerConfig)
            {
                ServiceDescription = "Test service - sender"
            };

            return service;
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
        public void SenderService_Send_DefaultQueueName_messageIsInTheQueue()
        {
            var senderService = GetSenderService();

            senderService.Send("message");

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

        //-----------------------

        [Test]
        public void SenderService_Send_ToExchange_messageIsInTheQueues()
        {
            var senderService = GetSenderService();

            senderService.Send("message", exchangeName, "", Encoding.UTF8, false);

            var totalMessagesInQueue1 = _rabbitMqHelper.GetTotalItems(queueNameExchange1);
            var totalMessagesInQueue2 = _rabbitMqHelper.GetTotalItems(queueNameExchange2);

            Assert.AreEqual(1, totalMessagesInQueue1);
            Assert.AreEqual(1, totalMessagesInQueue2);
        }


        [Test]
        public void SenderService_Send_ToExchange_DefaultExchangeName_messageIsInTheQueues()
        {
            var config = _brokerConfig;
            config.Exchange = exchangeName;

            var senderService = GetSenderService(config);

            senderService.Send("message");

            var totalMessagesInQueue1 = _rabbitMqHelper.GetTotalItems(queueNameExchange1);
            var totalMessagesInQueue2 = _rabbitMqHelper.GetTotalItems(queueNameExchange2);

            Assert.AreEqual(1, totalMessagesInQueue1);
            Assert.AreEqual(1, totalMessagesInQueue2);
        }

    }
}
