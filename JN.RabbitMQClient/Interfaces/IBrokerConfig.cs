namespace JN.RabbitMQClient.Interfaces
{
    public interface IBrokerConfig
    {
        string Host { get; set; }
        string Username { get; set; }
        string Password { get; set; }
        string VirtualHost { get; set; }
        int Port { get; set; }
        bool ShuffleHostList { get; set; }
        bool UseTLS { get; set; }
        int ConnectionTimeoutSeconds { get; set; }
    }

    public interface IBrokerConfigSender : IBrokerConfig
    {
        string RoutingKeyOrQueueName { get; set; }
        string Exchange { get; set; }
        bool KeepConnectionOpen { get; set; }
        bool GetQueueInfoOnSend { get; set; }
    }

    public interface IBrokerConfigConsumers : IBrokerConfig
    {
        string RoutingKeyOrQueueName { get; set; }
        byte TotalInstances { get; set; }
    }


}