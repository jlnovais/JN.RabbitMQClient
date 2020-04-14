namespace JN.RabbitMQClient.Entities
{
    public class RetryQueueDetails
    {
        /// <summary>
        /// Retry queue name
        /// </summary>
        public string RetryQueue { get; set; }

        /// <summary>
        /// This is the message expiration
        /// </summary>
        public int RetentionPeriodInRetryQueueMilliseconds { get; set; }

        /// <summary>
        /// This is the message expiration - max value; if specified, a random value between RetentionPeriodInRetryQueueMilliseconds and RetentionPeriodInRetryQueueMillisecondsMax will be used
        /// </summary>
        public int RetentionPeriodInRetryQueueMillisecondsMax { get; set; }
    }
}
