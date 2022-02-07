using JN.RabbitMQClient.Interfaces;
using NUnit.Framework;

namespace JN.RabbitMQClient.Tests.Integration
{
    public class RabbitMqTlsConnectionTests
    {
        private const string host = "rabbit.josenovais.com";
        private const string hostNameNotMatchingCertificate = "serverA";
        private const string queueName = "TestQueue_created";

        private readonly IBrokerConfig _brokerConfig = new Entities.BrokerConfig
        {
            Username = "test",
            Password = "123",
            Host = host,
            VirtualHost = "/",
            UseTLS = true
        };

        private readonly IBrokerConfig _brokerConfigHostNameNotMatchingCertificate = new Entities.BrokerConfig
        {
            Username = "test",
            Password = "123",
            Host = hostNameNotMatchingCertificate,
            VirtualHost = "/",
            UseTLS = true

        };


        [Test]
        public void SenderService_CreateQueue_returnsOk()
        {
            var service = GetService();

            var resultCreateQueue = service.CreateQueue(queueName);
            Assert.IsTrue(resultCreateQueue.Success);

            var resultDeleteQueue = service.DeleteQueue(queueName);
            Assert.IsTrue(resultDeleteQueue.Success);
        }

        [Test]
        public void SenderService_CreateQueue_hostNameNotMatchingCertificate_returnsError()
        {
            var service = GetService(_brokerConfigHostNameNotMatchingCertificate);

            var resultCreateQueue = service.CreateQueue(queueName);
            Assert.IsFalse(resultCreateQueue.Success);
        }

        private IRabbitMqUtilitiesService GetService(IBrokerConfig config = null)
        {
            var service = new RabbitMqUtilitiesService(config ?? _brokerConfig)
            {
                ServiceDescription = "Test service - sender"
            };

            return service;
        }
    }
}