using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQClient.Interfaces;

namespace JN.RabbitMQClient
{
    public class Sender: RabbitMQClientBase, ISender
    {
        //private bool _isInitialized;
        public string ExchangeName { get; set; }


        public Sender()
        {
            ExchangeName = "";
        }

        /// <summary>
        /// Send a message.
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <param name="createQueue">Try to create the queue - optional.</param>
        public void Send(string message, bool createQueue = false)
        {
            Send(message, this.ExchangeName, this.QueueName, createQueue);
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

            SetupConnection();

            var properties = _model.CreateBasicProperties();
            Tools.SetPropertiesSender(properties);

            byte[] messageBuffer = encoding.GetBytes(message);

            if (string.IsNullOrWhiteSpace(exchangeName) && createQueue)
            {
                try
                {
                    _model.QueueDeclare(routingKeyOrQueueName, true, false, false, null);
                }
                catch (Exception e)
                {
                    throw new Exception("Unable do create queue.", e);
                }

            }

            /* 
             * to send to an exchange:
             * 
             * exchange = [exchange name]
             * routing key= [routing key]
             * 
             * to send to a queue
             * 
             * exchange = "" (this means: use the default system exchange)
             * routing key= [queue name]
             */

            _model.BasicPublish(
                (string.IsNullOrWhiteSpace(exchangeName) ? ExchangeName : exchangeName),
                (string.IsNullOrWhiteSpace(routingKeyOrQueueName) ? QueueName : routingKeyOrQueueName),
                properties,
                messageBuffer);

            Dispose();
        }


    }
}
