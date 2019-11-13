using System.Text;
using RabbitMQ.Client;

namespace JN.RabbitMQClient.Tests.HelperClasses
{
    public class RabbitMqHelper
    {
        private readonly BrokerConfig _config;

        public RabbitMqHelper(BrokerConfig config)
        {
            _config = config;
        }

        public void DeleteQueue(string queueName)
        {
            var connectionFactory = new ConnectionFactory
            {
                HostName = _config.HostName, 
                UserName = _config.UserName, 
                Password = _config.Password,
                VirtualHost = _config.VirtualHost
            };
            var connection = connectionFactory.CreateConnection();
            var channel = connection.CreateModel();
            channel.QueueDelete(queueName);
            connection.Close();
        }

        public QueueDeclareOk CreateQueueOrGetInfo(string queueName)
        {
            var connectionFactory = new ConnectionFactory
            {
                HostName = _config.HostName,
                UserName = _config.UserName,
                Password = _config.Password,
                VirtualHost = _config.VirtualHost
            };
            var connection = connectionFactory.CreateConnection();
            var channel = connection.CreateModel();
            var res = channel.QueueDeclare(queueName, true, false, false);
            connection.Close();

            return res;
        }

        public void SendMessage(string queueName, string message)
        {
            var connectionFactory = new ConnectionFactory
            {
                HostName = _config.HostName,
                UserName = _config.UserName,
                Password = _config.Password,
                VirtualHost = _config.VirtualHost
            };
            var connection = connectionFactory.CreateConnection();
            var channel = connection.CreateModel();

            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(
                "",
                queueName,
                null,
                body);

            connection.Close();
        }
    }
}
