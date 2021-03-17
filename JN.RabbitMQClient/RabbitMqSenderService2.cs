using System;
using System.Text;
using JN.RabbitMQClient.Interfaces;
using RabbitMQ.Client;

namespace JN.RabbitMQClient
{
    /// <summary>
    /// Service for sending messages - keeps connection open
    /// </summary>
    public class RabbitMqSenderService2 : RabbitMqSenderService, IRabbitMqSenderServiceKeepConnection, IDisposable
    {
        private IConnection _connection;
        private IModel _channel;

        public bool IsConnected => _connection != null && _connection.IsOpen;

        public RabbitMqSenderService2(IBrokerConfigSender config) : base(config)
        {
            
        }
        
        private void CloseConnection()
        {
            _connection?.Dispose();
            _channel?.Dispose();
        }

        public void SetupConnection()
        {
            if (IsConnected) 
                return;
            
            _connection = GetConnection(ServiceDescription + "_sender", false);
            _channel = _connection.CreateModel();
        }

        /// <summary>
        /// Send message using custom encoding. To send to an exchange: exchange = [exchange name], routing key= [routing key]. To send to a queue: exchange = "", routing key= [queue name].
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <param name="exchangeName">Name of the exchange to which the message will be sent.</param>
        /// <param name="routingKeyOrQueueName">Routing key or queue name to which the message will be sent.</param>
        /// <param name="encoding">Message encoding.</param>
        /// <param name="createQueue">Try to create the queue (when sending to a queue) - optional.</param>
        public override void Send(string message, string exchangeName, string routingKeyOrQueueName, Encoding encoding, bool createQueue)
        {
            if (!IsConnected)
            {
                SetupConnection();
            }
            
            var config = (IBrokerConfigSender) _config;

            var body = encoding.GetBytes(message);

            var encodingName = encoding.EncodingName;

            _Send(exchangeName, routingKeyOrQueueName, createQueue, _channel, encodingName, config, body);

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

        ~RabbitMqSenderService2()
        {
            Dispose(false);
        }
    }
}