using System;
using System.Text;
using JN.RabbitMQClient.Interfaces;
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
        /// <param name="message"></param>
        public void Send(string message)
        {
            Send(message, null, null);
        }

        /// <summary>
        /// Send message using default encoding. To send to an exchange: exchange = [exchange name], routing key= [routing key]. To send to a queue: exchange = "", routing key= [queue name].
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exchangeName"></param>
        /// <param name="routingKeyOrQueueName"></param>
        public void Send(string message, string exchangeName, string routingKeyOrQueueName)
        {
            var encoding = Encoding.UTF8;

            Send(message, exchangeName, routingKeyOrQueueName, encoding);
        }

        /// <summary>
        /// Send message using custom encoding by name (ex: utf-8, utf-7, utf-32, utf-16, iso-8859-1, etc). To send to an exchange: exchange = [exchange name], routing key= [routing key]. To send to a queue: exchange = "", routing key= [queue name].
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exchangeName"></param>
        /// <param name="routingKeyOrQueueName"></param>
        /// <param name="encodingName"></param>
        public void Send(string message, string exchangeName, string routingKeyOrQueueName, string encodingName)
        {
            var encoding = Encoding.GetEncoding(encodingName);

            Send(message, exchangeName, routingKeyOrQueueName, encoding);
        }

        /// <summary>
        /// Send message using custom encoding. To send to an exchange: exchange = [exchange name], routing key= [routing key]. To send to a queue: exchange = "", routing key= [queue name].
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exchangeName"></param>
        /// <param name="routingKeyOrQueueName"></param>
        /// <param name="encoding"></param>
        public void Send(string message, string exchangeName, string routingKeyOrQueueName, Encoding encoding)
        {
            var body = encoding.GetBytes(message);

            using (var connection = GetConnection(ServiceDescription + "_sender"))
            using (var channel = connection.CreateModel())
            {
                var properties = channel.CreateBasicProperties();
                SetPropertiesSender(properties, encoding.EncodingName);

                var exchange = string.IsNullOrWhiteSpace(exchangeName) ? _config.Exchange : exchangeName;
                if (string.IsNullOrWhiteSpace(exchangeName))
                    exchange = "";

                channel.BasicPublish(
                    exchange,
                    (string.IsNullOrWhiteSpace(routingKeyOrQueueName)
                        ? _config.RoutingKeyOrQueueName
                        : routingKeyOrQueueName),
                    properties,
                    body);
            }
        }

        internal static void SetPropertiesSender(IBasicProperties properties, string encoding)
        {
            properties.Persistent = true; // SetPersistent(true);
            properties.Timestamp = new AmqpTimestamp(DateTime.UtcNow.ToUnixTimestamp());
            properties.ContentEncoding = encoding;
        }


    }
}
