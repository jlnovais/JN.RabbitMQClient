using System.Text;

namespace JN.RabbitMQClient.Interfaces
{
    public interface IRabbitMqSenderService
    {
        /// <summary>
        /// Send message using default settings
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <param name="properties">Message properties (optional)</param>
        void Send(string message, IMessageProperties properties = null);


        /// <summary>
        /// Send message using default settings
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <param name="createQueue">Try to create the queue - optional.</param>
        /// <param name="properties">Message properties (optional)</param>
        void Send(string message, bool createQueue, IMessageProperties properties = null);

        /// <summary>
        /// Send message using default encoding. To send to an exchange: exchange = [exchange name], routing key= [routing key]. To send to a queue: exchange = "", routing key= [queue name].
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <param name="exchangeName">Name of the exchange to which the message will be sent.</param>
        /// <param name="routingKeyOrQueueName">Routing key or queue name to which the message will be sent.</param>
        /// <param name="properties">Message properties (optional)</param>
        void Send(string message, string exchangeName, string routingKeyOrQueueName,
            IMessageProperties properties = null);

        /// <summary>
        /// Send message using default encoding. To send to an exchange: exchange = [exchange name], routing key= [routing key]. To send to a queue: exchange = "", routing key= [queue name].
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <param name="exchangeName">Name of the exchange to which the message will be sent.</param>
        /// <param name="routingKeyOrQueueName">Routing key or queue name to which the message will be sent.</param>
        /// <param name="createQueue">Try to create the queue (when sending to a queue) - optional.</param>
        /// <param name="properties">Message properties (optional)</param>
        void Send(string message, string exchangeName, string routingKeyOrQueueName, bool createQueue,
            IMessageProperties properties = null);


        /// <summary>
        /// Send message using custom encoding by name (ex: utf-8, utf-7, utf-32, utf-16, iso-8859-1, etc). To send to an exchange: exchange = [exchange name], routing key= [routing key]. To send to a queue: exchange = "", routing key= [queue name].
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <param name="exchangeName">Name of the exchange to which the message will be sent.</param>
        /// <param name="routingKeyOrQueueName">Routing key or queue name to which the message will be sent.</param>
        /// <param name="encodingName">Message encoding name.</param>
        /// <param name="properties">Message properties (optional)</param>
        void Send(string message, string exchangeName, string routingKeyOrQueueName, string encodingName,
            IMessageProperties properties = null);

        /// <summary>
        /// Send message using custom encoding by name (ex: utf-8, utf-7, utf-32, utf-16, iso-8859-1, etc). To send to an exchange: exchange = [exchange name], routing key= [routing key]. To send to a queue: exchange = "", routing key= [queue name].
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <param name="exchangeName">Name of the exchange to which the message will be sent.</param>
        /// <param name="routingKeyOrQueueName">Routing key or queue name to which the message will be sent.</param>
        /// <param name="encodingName">Message encoding name.</param>
        /// <param name="createQueue">Try to create the queue (when sending to a queue) - optional.</param>
        /// <param name="properties">Message properties (optional)</param>
        void Send(string message, string exchangeName, string routingKeyOrQueueName, string encodingName,
            bool createQueue, IMessageProperties properties = null);

        /// <summary>
        /// Send message using custom encoding. To send to an exchange: exchange = [exchange name], routing key= [routing key]. To send to a queue: exchange = "", routing key= [queue name].
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <param name="exchangeName">Name of the exchange to which the message will be sent.</param>
        /// <param name="routingKeyOrQueueName">Routing key or queue name to which the message will be sent.</param>
        /// <param name="encoding">Message encoding.</param>
        /// <param name="properties">Message properties (optional)</param>
        void Send(string message, string exchangeName, string routingKeyOrQueueName, Encoding encoding,
            IMessageProperties properties = null);


        /// <summary>
        /// Send message using custom encoding. To send to an exchange: exchange = [exchange name], routing key= [routing key]. To send to a queue: exchange = "", routing key= [queue name].
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <param name="exchangeName">Name of the exchange to which the message will be sent.</param>
        /// <param name="routingKeyOrQueueName">Routing key or queue name to which the message will be sent.</param>
        /// <param name="encoding">Message encoding.</param>
        /// <param name="createQueue">Try to create the queue (when sending to a queue) - optional.</param>
        /// <param name="properties">Message properties (optional)</param>
        void Send(string message, string exchangeName, string routingKeyOrQueueName, Encoding encoding,
            bool createQueue, IMessageProperties properties = null);

        string ServiceDescription { get; set; }

        bool IsConnected { get; }

    }
}