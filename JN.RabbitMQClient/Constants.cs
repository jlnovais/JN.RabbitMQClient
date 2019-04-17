namespace JN.RabbitMQClient
{
    public static class Constants
    {
        public enum MessageProcessInstruction
        {
            OK,
            RequeueMessageWithDelay,
            IgnoreMessage,
            IgnoreMessageWithRequeue,
            Unknown
        };

        internal const string FirstErrorTimeStampHeaderName = "firstErrorTimeStamp";
    }
}
