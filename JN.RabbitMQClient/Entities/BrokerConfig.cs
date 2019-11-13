using JN.RabbitMQClient.Interfaces;

namespace JN.RabbitMQClient.Entities
{
    public class BrokerConfig : IBrokerConfigSender, IBrokerConfigConsumers
    {
        public string Host { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string VirtualHost { get; set; }
        public int Port { get; set; }
        public string RoutingKeyOrQueueName { get; set; }
        public string Exchange { get; set; }
        public short TotalInstances { get; set; }
        public bool ShuffleHostList { get; set; }
    }


}
