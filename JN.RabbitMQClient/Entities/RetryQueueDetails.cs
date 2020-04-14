namespace JN.RabbitMQClient.Entities
{
    public class RetryQueueDetails
    {
        public string RetryQueue { get; set; }
        public int RetentionPeriodInRetryQueueMilliseconds { get; set; }
        public int RetentionPeriodInRetryQueueMillisecondsMax { get; set; }
    }
}
