using JN.RabbitMQClient.Entities;

namespace JN.RabbitMQClient.Interfaces
{
    public interface IRabbitMqUtilitiesService
    {
        Result CreateQueue(string queueName);
        Result CreateQueue(string queueName, string exchangeToBind);
        Result CreateQueue(string queueName, string exchangeToBind, string bindRoutingKey);
        Result<uint> GetTotalMessages(string queueName);
        Result DeleteQueue(string queueName);
    }
}
