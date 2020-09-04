namespace JN.RabbitMQClient.Limiter
{
    public interface ILimiter
    {

        Constants.MessageProcessInstruction DeniedProcessInstruction { get; }

        bool IsAllowed(string routingKeyOrQueueName, string consumerTag, long firstErrorTimestamp, string exchange, string message);
    }
}