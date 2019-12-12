using System;
using System.Text;
using JN.RabbitMQClient.Interfaces;
using JN.RabbitMQClient.Other;
using RabbitMQ.Client;

namespace JN.RabbitMQClient
{
    public class RabbitMqSenderService : RabbitMqServiceBase, IRabbitMqSenderService
    {
        public RabbitMqSenderService(IBrokerConfigSender config)
        {
            _config = config;
        }


        /// <summary>
        /// Send message using default settings
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <param name="createQueue">Try to create the queue - optional.</param>
        public void Send(string message, bool createQueue = false)
        {
            Send(message, null, null, createQueue);
        }

        /// <summary>
        /// Send message using default encoding. To send to an exchange: exchange = [exchange name], routing key= [routing key]. To send to a queue: exchange = "", routing key= [queue name].
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <param name="exchangeName">Name of the exchange to which the message will be sent.</param>
        /// <param name="routingKeyOrQueueName">Routing key or queue name to which the message will be sent.</param>
        /// <param name="createQueue">Try to create the queue (when sending to a queue) - optional.</param>
        public void Send(string message, string exchangeName, string routingKeyOrQueueName, bool createQueue = false)
        {
            var encoding = Encoding.UTF8;

            Send(message, exchangeName, routingKeyOrQueueName, encoding, createQueue);
        }

        /// <summary>
        /// Send message using custom encoding by name (ex: utf-8, utf-7, utf-32, utf-16, iso-8859-1, etc). To send to an exchange: exchange = [exchange name], routing key= [routing key]. To send to a queue: exchange = "", routing key= [queue name].
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <param name="exchangeName">Name of the exchange to which the message will be sent.</param>
        /// <param name="routingKeyOrQueueName">Routing key or queue name to which the message will be sent.</param>
        /// <param name="encodingName">Message encoding name.</param>
        /// <param name="createQueue">Try to create the queue (when sending to a queue) - optional.</param>
        public void Send(string message, string exchangeName, string routingKeyOrQueueName, string encodingName, bool createQueue = false)
        {
            var encoding = Encoding.GetEncoding(encodingName);

            Send(message, exchangeName, routingKeyOrQueueName, encoding, createQueue);
        }

        /// <summary>
        /// Send message using custom encoding. To send to an exchange: exchange = [exchange name], routing key= [routing key]. To send to a queue: exchange = "", routing key= [queue name].
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <param name="exchangeName">Name of the exchange to which the message will be sent.</param>
        /// <param name="routingKeyOrQueueName">Routing key or queue name to which the message will be sent.</param>
        /// <param name="encoding">Message encoding.</param>
        /// <param name="createQueue">Try to create the queue (when sending to a queue) - optional.</param>
        public void Send(string message, string exchangeName, string routingKeyOrQueueName, Encoding encoding, bool createQueue = false)
        {
            var config = (IBrokerConfigSender)_config;

            var body = encoding.GetBytes(message);

            using (var connection = GetConnection(ServiceDescription + "_sender"))
            using (var channel = connection.CreateModel())
            {
                var properties = channel.CreateBasicProperties();
                RabbitMqUtilities.SetPropertiesSender(properties, encoding.EncodingName);

                var exchange = string.IsNullOrWhiteSpace(exchangeName) ? config.Exchange : exchangeName;

                if (string.IsNullOrWhiteSpace(exchange))
                    exchange = "";

                if (string.IsNullOrWhiteSpace(exchange) && createQueue)
                {
                    try
                    {
                        RabbitMqUtilities.CreateQueueOrGetInfo(routingKeyOrQueueName, channel);
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Unable do create queue.", e);
                    }

                }

                var routingKey = (string.IsNullOrWhiteSpace(routingKeyOrQueueName)
                    ? config.RoutingKeyOrQueueName
                    : routingKeyOrQueueName);

                if (string.IsNullOrWhiteSpace(routingKey))
                    routingKey = "";

                channel.BasicPublish(
                    exchange,
                    routingKey,
                    properties,
                    body);
            }
        }
    }
}
