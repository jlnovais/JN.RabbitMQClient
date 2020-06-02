using JN.RabbitMQClient.Interfaces;

namespace JN.RabbitMQClient.Entities
{
    public class BrokerConfig : IBrokerConfig
    {
        public string Host { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string VirtualHost { get; set; }
        public int Port { get; set; }
        public bool ShuffleHostList { get; set; }
        public bool UseTLS { get; set; }
    }

    public class BrokerConfigSender : BrokerConfig, IBrokerConfigSender
    {
        public string RoutingKeyOrQueueName { get; set; }
        public string Exchange { get; set; }
    }

    public class BrokerConfigConsumers : BrokerConfig, IBrokerConfigConsumers
    {
        public string RoutingKeyOrQueueName { get; set; }
        public byte TotalInstances { get; set; }
    }


}
