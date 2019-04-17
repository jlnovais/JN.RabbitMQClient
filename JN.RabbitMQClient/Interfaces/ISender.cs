using System.Text;

namespace RabbitMQClient.Interfaces
{
    public interface ISender
    {

        string ExchangeName { get; set; }
        void Send(string message);

        /// <summary>
        /// Send message using default encoding. To send to an exchange: exchange = [exchange name], routing key= [routing key]. To send to a queue: exchange = "", routing key= [queue name].
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exchangeName"></param>
        /// <param name="routingKeyOrQueueName"></param>
        void Send(string message, string exchangeName, string routingKeyOrQueueName);

        /// <summary>
        /// Send message using custom encoding by name (ex: utf-8, utf-7, utf-32, utf-16, iso-8859-1, etc). To send to an exchange: exchange = [exchange name], routing key= [routing key]. To send to a queue: exchange = "", routing key= [queue name].
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exchangeName"></param>
        /// <param name="routingKeyOrQueueName"></param>
        /// <param name="encodingName"></param>
        void Send(string message, string exchangeName, string routingKeyOrQueueName, string encodingName);

        /// <summary>
        /// Send message using custom encoding. To send to an exchange: exchange = [exchange name], routing key= [routing key]. To send to a queue: exchange = "", routing key= [queue name].
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exchangeName"></param>
        /// <param name="routingKeyOrQueueName"></param>
        /// <param name="encoding"></param>
        void Send(string message, string exchangeName, string routingKeyOrQueueName, Encoding encoding);


    }
}