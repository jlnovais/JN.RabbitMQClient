using System;
using System.Text;
using JN.RabbitMQClient.Entities;
using JN.RabbitMQClient.Interfaces;
using JN.RabbitMQClient.Other;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace JN.RabbitMQClient
{
    /// <summary>
    /// Service for sending messages
    /// </summary>
    public class RabbitMqSenderService : RabbitMqServiceBase, IRabbitMqSenderService, IDisposable
    {
        private IConnection _connection;
        private IModel _channel;

        private readonly object _lockObj = new object();
        public bool IsConnected => _connection != null && _connection.IsOpen && _channel !=null && _channel.IsOpen;


        public ConnectionDetails ConnectionDetails
        {
            get
            {
                var config = (IBrokerConfigSender)_config;

                return new ConnectionDetails
                {
                    Host = config.Host,
                    Port = config.Port,
                    UseTLS = config.UseTLS,
                    Username = config.Username,
                    VirtualHost = config.VirtualHost,
                    Exchange = config.Exchange,
                    RoutingKeyOrQueueName = config.RoutingKeyOrQueueName
                };
            }
        }

        public RabbitMqSenderService(IBrokerConfigSender config): base(config)
        {
        }


        /// <summary>
        /// Send message using default settings
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <param name="properties">Message properties (optional)</param>
        public void Send(string message, IMessageProperties properties = null)
        {
            Send(message, false, properties);
        }


        /// <summary>
        /// Send message using default settings
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <param name="createQueue">Try to create the queue - optional.</param>
        /// <param name="properties">Message properties (optional)</param>
        public void Send(string message, bool createQueue, IMessageProperties properties = null)
        {
            Send(message, null, null, createQueue, properties);
        }

        /// <summary>
        /// Send message using default encoding. To send to an exchange: exchange = [exchange name], routing key= [routing key]. To send to a queue: exchange = "", routing key= [queue name].
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <param name="exchangeName">Name of the exchange to which the message will be sent.</param>
        /// <param name="routingKeyOrQueueName">Routing key or queue name to which the message will be sent.</param>
        /// <param name="properties">Message properties (optional)</param>
        public void Send(string message, string exchangeName, string routingKeyOrQueueName, IMessageProperties properties = null)
        {
            var encoding = Encoding.UTF8;

            Send(message, exchangeName, routingKeyOrQueueName, encoding, false, properties);
        }

        /// <summary>
        /// Send message using default encoding. To send to an exchange: exchange = [exchange name], routing key= [routing key]. To send to a queue: exchange = "", routing key= [queue name].
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <param name="exchangeName">Name of the exchange to which the message will be sent.</param>
        /// <param name="routingKeyOrQueueName">Routing key or queue name to which the message will be sent.</param>
        /// <param name="createQueue">Try to create the queue (when sending to a queue) - optional.</param>
        /// <param name="properties">Message properties (optional)</param>
        public void Send(string message, string exchangeName, string routingKeyOrQueueName, bool createQueue, IMessageProperties properties = null)
        {
            var encoding = Encoding.UTF8;

            Send(message, exchangeName, routingKeyOrQueueName, encoding, createQueue, properties);
        }

        /// <summary>
        /// Send message using custom encoding by name (ex: utf-8, utf-7, utf-32, utf-16, iso-8859-1, etc). To send to an exchange: exchange = [exchange name], routing key= [routing key]. To send to a queue: exchange = "", routing key= [queue name].
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <param name="exchangeName">Name of the exchange to which the message will be sent.</param>
        /// <param name="routingKeyOrQueueName">Routing key or queue name to which the message will be sent.</param>
        /// <param name="encodingName">Message encoding name.</param>
        /// <param name="properties">Message properties (optional)</param>
        public void Send(string message, string exchangeName, string routingKeyOrQueueName, string encodingName, IMessageProperties properties = null)
        {
            var encoding = Encoding.GetEncoding(encodingName);

            Send(message, exchangeName, routingKeyOrQueueName, encoding, false, properties);
        }

        /// <summary>
        /// Send message using custom encoding by name (ex: utf-8, utf-7, utf-32, utf-16, iso-8859-1, etc). To send to an exchange: exchange = [exchange name], routing key= [routing key]. To send to a queue: exchange = "", routing key= [queue name].
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <param name="exchangeName">Name of the exchange to which the message will be sent.</param>
        /// <param name="routingKeyOrQueueName">Routing key or queue name to which the message will be sent.</param>
        /// <param name="encodingName">Message encoding name.</param>
        /// <param name="createQueue">Try to create the queue (when sending to a queue) - optional.</param>
        /// <param name="properties">Message properties (optional)</param>
        public void Send(string message, string exchangeName, string routingKeyOrQueueName, string encodingName, bool createQueue, IMessageProperties properties = null)
        {
            var encoding = Encoding.GetEncoding(encodingName);

            Send(message, exchangeName, routingKeyOrQueueName, encoding, createQueue, properties);
        }

        /// <summary>
        /// Send message using custom encoding. To send to an exchange: exchange = [exchange name], routing key= [routing key]. To send to a queue: exchange = "", routing key= [queue name].
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <param name="exchangeName">Name of the exchange to which the message will be sent.</param>
        /// <param name="routingKeyOrQueueName">Routing key or queue name to which the message will be sent.</param>
        /// <param name="encoding">Message encoding.</param>
        /// <param name="properties">Message properties (optional)</param>
        public void Send(string message, string exchangeName, string routingKeyOrQueueName, Encoding encoding, IMessageProperties properties = null)
        {
            Send(message, exchangeName, routingKeyOrQueueName, encoding, false, properties);
        }

        /// <summary>
        /// Send message using custom encoding. To send to an exchange: exchange = [exchange name], routing key= [routing key]. To send to a queue: exchange = "", routing key= [queue name].
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <param name="exchangeName">Name of the exchange to which the message will be sent.</param>
        /// <param name="routingKeyOrQueueName">Routing key or queue name to which the message will be sent.</param>
        /// <param name="encoding">Message encoding.</param>
        /// <param name="createQueue">Try to create the queue (when sending to a queue) - optional.</param>
        /// <param name="properties">Message properties (optional)</param>
        public void Send(string message, string exchangeName, string routingKeyOrQueueName, Encoding encoding, bool createQueue, IMessageProperties properties = null)
        {
            var config = (IBrokerConfigSender)_config;

            var msgProperties = GetMessageProperties(encoding, properties);
            
            var encodingFromProperties = Encoding.GetEncoding(msgProperties.ContentEncoding);

            var body = encodingFromProperties.GetBytes(message);

            if (config.KeepConnectionOpen)
            {
                if (!IsConnected)
                {
                    SetupConnection();
                }

                _Send(exchangeName, routingKeyOrQueueName, createQueue, _channel, config, body, msgProperties);
            }

            else
            {
                //create and dispose connection here
                using (var connection = GetConnection(ServiceDescription + "_sender"))
                using (var channel = connection.CreateModel())
                {
                    _Send(exchangeName, routingKeyOrQueueName, createQueue, channel, config, body, msgProperties);
                }
            }
        }

 
        private IMessageProperties GetMessageProperties(Encoding encoding, IMessageProperties properties)
        {
            if (properties is null)
            {
                return new MessageProperties
                {
                    ContentEncoding = encoding.HeaderName,
                    Persistent = true
                };
            }

            if (string.IsNullOrWhiteSpace(properties.ContentEncoding))
                properties.ContentEncoding = encoding.HeaderName;

            return properties;
        }


        protected void _Send(string exchangeName, string routingKeyOrQueueName, bool createQueue, IModel channel,
             IBrokerConfigSender config, byte[] body, IMessageProperties msgProperties)
        {
            var properties = channel.CreateBasicProperties();

            RabbitMqUtilities.SetProperties(properties, msgProperties);

            var exchange = GetExchange(exchangeName, config);

            CreateQueue(routingKeyOrQueueName, createQueue, exchange, channel);

            var routingKey = GetRoutingKey(routingKeyOrQueueName, config);

            if (channel.IsClosed)
                throw new RabbitMqClientException("Channel is closed");

            channel.BasicPublish(
                exchange,
                routingKey,
                properties,
                body);
        }

        protected static void CreateQueue(string routingKeyOrQueueName, bool create, string exchange, IModel channel)
        {
            if (string.IsNullOrWhiteSpace(exchange) && create)
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
            var routingKey = string.IsNullOrWhiteSpace(routingKeyOrQueueName)
                ? config.RoutingKeyOrQueueName
                : routingKeyOrQueueName;

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

        ~RabbitMqSenderService()
        {
            Dispose(false);
        }
    }
}