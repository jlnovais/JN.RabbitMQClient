using System.Text;

namespace JN.RabbitMQClient
{
    public interface IRabbitMqSenderService
    {
        /// <summary>
        /// Send message using default settings
        /// </summary>
        /// <param name="message"></param>
        void Send(string message);

        /// <summary>
        /// Send message using default enconding. To send to an exchange: exchange = [exchange name], routing key= [routing key]. To send to a queue: exchange = "", routing key= [queue name].
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exchangeName"></param>
        /// <param name="routingKeyOrQueueName"></param>
        void Send(string message, string exchangeName, string routingKeyOrQueueName);

        /// <summary>
        /// Send message using custom enconding by name (ex: utf-8, utf-7, utf-32, utf-16, iso-8859-1, etc). To send to an exchange: exchange = [exchange name], routing key= [routing key]. To send to a queue: exchange = "", routing key= [queue name].
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exchangeName"></param>
        /// <param name="routingKeyOrQueueName"></param>
        /// <param name="encodingName"></param>
        void Send(string message, string exchangeName, string routingKeyOrQueueName, string encodingName);

        /// <summary>
        /// Send message using custom enconding. To send to an exchange: exchange = [exchange name], routing key= [routing key]. To send to a queue: exchange = "", routing key= [queue name].
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exchangeName"></param>
        /// <param name="routingKeyOrQueueName"></param>
        /// <param name="encoding"></param>
        void Send(string message, string exchangeName, string routingKeyOrQueueName, Encoding encoding);

        string ServiceDescription { get; set; }

    }
}