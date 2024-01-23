using JN.RabbitMQClient.Entities;
using System;

namespace JN.RabbitMQClient.Interfaces
{
    public interface IRabbitMqUtilitiesService
    {
        Result CreateQueue(string queueName);
        Result CreateQueue(string queueName, string exchangeToBind);
        Result CreateQueue(string queueName, string exchangeToBind, string bindRoutingKey);
        [Obsolete("GetTotalMessages is deprecated, please use GetQueueInfo instead.")]
        Result<uint> GetTotalMessages(string queueName);
        Result DeleteQueue(string queueName);
        Result<QueueInfo> GetQueueInfo(string queueName);
    }
}
