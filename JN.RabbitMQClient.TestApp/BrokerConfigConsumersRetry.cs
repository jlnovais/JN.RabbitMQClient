namespace JN.RabbitMQClient.TestApp
{
    public class BrokerConfigConsumersRetry
    {
        public int BrokerMessageTTLSeconds { get; set; }
        public string BrokerRetryQueue { get; set; }
        public int BrokerRetentionPeriodInRetryQueueSeconds { get; set; }
        public int BrokerRetentionPeriodInRetryQueueSecondsMax { get; set; }
    }
}