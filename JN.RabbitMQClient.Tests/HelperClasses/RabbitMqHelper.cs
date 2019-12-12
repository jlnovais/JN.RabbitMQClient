using System.Collections.Generic;
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


        public void CreateExchangeAndBindQueue(string exName, IList<string> bindQueues)
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
            channel.ExchangeDeclare(exName, "fanout", true, false, null);

            if (bindQueues != null)
            {
                foreach (var bindQueue in bindQueues)
                {
                    channel.QueueBind(bindQueue, exName, "");
                }
            }
            connection.Close();
        }

        public void DeleteExchangeAndBindedQueues(string exName, IList<string> bindQueues)
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


            if (bindQueues != null)
            {
                foreach (var bindQueue in bindQueues)
                {
                    channel.QueueDelete(bindQueue, false, false);
                }
            }

            channel.ExchangeDelete(exName, false);

            connection.Close();
        }


        public uint GetTotalItems(string queueName)
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
            var res = channel.MessageCount(queueName);
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
