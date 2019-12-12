using JN.RabbitMQClient.Entities;

namespace JN.RabbitMQClient.Interfaces
{
    public interface IRabbitMqUtilitiesService
    {
        Result CreateQueue(string queueName, string exchangeToBind = null, string bindRoutingKey = null);
        Result<uint> GetTotalMessages(string queueName);
        Result DeleteQueue(string queueName);
    }
}
