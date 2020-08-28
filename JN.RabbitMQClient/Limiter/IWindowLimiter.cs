using System;

namespace JN.RabbitMQClient.Limiter
{
    public interface IWindowLimiter : ILimiterBase
    {
        DateTime Start { get; }
        DateTime End { get; }
        int MaxAllowed { get; }
        int WindowCount { get; }
        int WindowSeconds { get; }
    }
}