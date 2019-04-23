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

        public void Send(string message)
        {
            Send(message, null, null);
        }


        
        /// <summary>
        /// Send message using default enconding. To send to an exchange: exchange = [exchange name], routing key= [routing key]. To send to a queue: exchange = "", routing key= [queue name].
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
        /// Send message using custom enconding. To send to an exchange: exchange = [exchange name], routing key= [routing key]. To send to a queue: exchange = "", routing key= [queue name].
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exchangeName"></param>
        /// <param name="routingKeyOrQueueName"></param>
        /// <param name="encoding"></param>
        public void Send(string message, string exchangeName, string routingKeyOrQueueName, Encoding encoding)
        {

            SetupConnection();

            var properties = _model.CreateBasicProperties();
            Tools.SetPropertiesSender(properties);

            byte[] messageBuffer = encoding.GetBytes(message);

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
