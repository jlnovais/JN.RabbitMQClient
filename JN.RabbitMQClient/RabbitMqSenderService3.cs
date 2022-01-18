using System;
using System.Text;
using JN.RabbitMQClient.Entities;
using JN.RabbitMQClient.Interfaces;
using JN.RabbitMQClient.Other;
using RabbitMQ.Client;

namespace JN.RabbitMQClient
{
    /// <summary>
    /// Service for sending messages - keeps connection open
    /// </summary>
    public class RabbitMqSenderService3 : RabbitMqServiceBase, IRabbitMqSenderService, IDisposable
    {
        private IConnection _connection;
        private IModel _channel;

        private readonly object _lockObj = new object();
        public bool IsConnected => _connection != null && _connection.IsOpen && _channel !=null && _channel.IsOpen;


        public RabbitMqSenderService3(IBrokerConfigSender config)
        {
            _config = config;
            
        }


        /// <summary>
        /// Send message using default settings
        /// </summary>
        /// <param name="message">Message to send.</param>
        public void Send(string message)
        {
            Send(message, false);
        }


        /// <summary>
        /// Send message using default settings
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <param name="createQueue">Try to create the queue - optional.</param>
        public void Send(string message, bool createQueue)
        {
            Send(message, null, null, createQueue);
        }

        /// <summary>
        /// Send message using default encoding. To send to an exchange: exchange = [exchange name], routing key= [routing key]. To send to a queue: exchange = "", routing key= [queue name].
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <param name="exchangeName">Name of the exchange to which the message will be sent.</param>
        /// <param name="routingKeyOrQueueName">Routing key or queue name to which the message will be sent.</param>
        public void Send(string message, string exchangeName, string routingKeyOrQueueName)
        {
            var encoding = Encoding.UTF8;

            Send(message, exchangeName, routingKeyOrQueueName, encoding, false);
        }

        /// <summary>
        /// Send message using default encoding. To send to an exchange: exchange = [exchange name], routing key= [routing key]. To send to a queue: exchange = "", routing key= [queue name].
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <param name="exchangeName">Name of the exchange to which the message will be sent.</param>
        /// <param name="routingKeyOrQueueName">Routing key or queue name to which the message will be sent.</param>
        /// <param name="createQueue">Try to create the queue (when sending to a queue) - optional.</param>
        public void Send(string message, string exchangeName, string routingKeyOrQueueName, bool createQueue)
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
        public void Send(string message, string exchangeName, string routingKeyOrQueueName, string encodingName)
        {
            var encoding = Encoding.GetEncoding(encodingName);

            Send(message, exchangeName, routingKeyOrQueueName, encoding, false);
        }

        /// <summary>
        /// Send message using custom encoding by name (ex: utf-8, utf-7, utf-32, utf-16, iso-8859-1, etc). To send to an exchange: exchange = [exchange name], routing key= [routing key]. To send to a queue: exchange = "", routing key= [queue name].
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <param name="exchangeName">Name of the exchange to which the message will be sent.</param>
        /// <param name="routingKeyOrQueueName">Routing key or queue name to which the message will be sent.</param>
        /// <param name="encodingName">Message encoding name.</param>
        /// <param name="createQueue">Try to create the queue (when sending to a queue) - optional.</param>
        public void Send(string message, string exchangeName, string routingKeyOrQueueName, string encodingName, bool createQueue)
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
        public void Send(string message, string exchangeName, string routingKeyOrQueueName, Encoding encoding)
        {
            Send(message, exchangeName, routingKeyOrQueueName, encoding, false);
        }

        /// <summary>
        /// Send message using custom encoding. To send to an exchange: exchange = [exchange name], routing key= [routing key]. To send to a queue: exchange = "", routing key= [queue name].
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <param name="exchangeName">Name of the exchange to which the message will be sent.</param>
        /// <param name="routingKeyOrQueueName">Routing key or queue name to which the message will be sent.</param>
        /// <param name="encoding">Message encoding.</param>
        /// <param name="createQueue">Try to create the queue (when sending to a queue) - optional.</param>
        public void Send(string message, string exchangeName, string routingKeyOrQueueName, Encoding encoding,
            bool createQueue)
        {
            var config = (IBrokerConfigSender)_config;


            var body = encoding.GetBytes(message);

            var encodingName = encoding.EncodingName;

            if (config.KeepConnectionOpen)
            {
                if (!IsConnected)
                {
                    SetupConnection();
                }

                _Send(exchangeName, routingKeyOrQueueName, createQueue, _channel, encodingName, config, body);
            }

            else
            {
                using (var connection = GetConnection(ServiceDescription + "_sender"))
                using (var channel = connection.CreateModel())
                {
                    _Send(exchangeName, routingKeyOrQueueName, createQueue, channel, encodingName, config, body);
                }
            }


        }
        
        protected void _Send(string exchangeName, string routingKeyOrQueueName, bool createQueue, IModel channel,
            string encodingName, IBrokerConfigSender config, byte[] body)
        {
            var properties = channel.CreateBasicProperties();

            RabbitMqUtilities.SetPropertiesSender(properties, encodingName);

            var exchange = GetExchange(exchangeName, config);

            CreateQueue(routingKeyOrQueueName, createQueue, exchange, channel);

            var routingKey = GetRoutingKey(routingKeyOrQueueName, config);

            if (channel.IsClosed)
                throw new Exception("Channel is closed");

            channel.BasicPublish(
                exchange,
                routingKey,
                properties,
                body);
        }

        protected static void CreateQueue(string routingKeyOrQueueName, bool createQueue, string exchange, IModel channel)
        {
            if (string.IsNullOrWhiteSpace(exchange) && createQueue)
            {
                try
                {
                    RabbitMqUtilities.CreateQueueOrGetInfo(routingKeyOrQueueName, channel);
                }
                catch (Exception e)
                {
                    throw new RabbitMqClientException($"Unable do create queue. Details: {e.Message}", e);
                }
            }
        }

        protected string GetRoutingKey(string routingKeyOrQueueName, IBrokerConfigSender config)
        {
            var routingKey = (string.IsNullOrWhiteSpace(routingKeyOrQueueName)
                ? config.RoutingKeyOrQueueName
                : routingKeyOrQueueName);

            if (string.IsNullOrWhiteSpace(routingKey))
            {
                routingKey = "";
            }

            return routingKey;
        }

        protected string GetExchange(string exchangeName, IBrokerConfigSender config)
        {
            var exchange = string.IsNullOrWhiteSpace(exchangeName) ? config.Exchange : exchangeName;

            if (string.IsNullOrWhiteSpace(exchange))
                exchange = "";
            return exchange;
        }


        private void CloseConnection()
        {
            var config = (IBrokerConfigSender)_config;

            if (!config.KeepConnectionOpen)
                return;

            _connection?.Dispose();
            _channel?.Dispose();
        }

        public void SetupConnection()
        {
            lock (_lockObj)
            {
                if (IsConnected)
                    return;

                CloseConnection();
                
                _connection = GetConnection(ServiceDescription + "_sender", false);
                _channel = _connection.CreateModel();
            }
        }



        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                CloseConnection();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~RabbitMqSenderService3()
        {
            Dispose(false);
        }
    }
}