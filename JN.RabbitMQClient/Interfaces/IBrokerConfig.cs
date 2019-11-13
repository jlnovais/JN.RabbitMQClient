namespace JN.RabbitMQClient.Interfaces
{
    public interface IBrokerConfig
    {
        string Host { get; set; }
        string Username { get; set; }
        string Password { get; set; }
        string VirtualHost { get; set; }
        int Port { get; set; }
        string RoutingKeyOrQueueName { get; set; }
        string Exchange { get; set; }
        short TotalInstances { get; set; }
        bool ShuffleHostList { get; set; } 
    }

    public interface IBrokerConfigSender : IBrokerConfig
    {

    }

    public interface IBrokerConfigConsumers : IBrokerConfig
    {
        
    }


}