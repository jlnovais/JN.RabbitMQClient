namespace JN.RabbitMQClient.Limiter
{
    public interface ILimiterBase
    {
        bool IsAllowed();

        Constants.MessageProcessInstruction AllowedProcessInstruction { get; }
        Constants.MessageProcessInstruction DeniedProcessInstruction { get; }

    }
}