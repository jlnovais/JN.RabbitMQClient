using System.Collections.Generic;
using JN.RabbitMQClient.Entities;

namespace JN.RabbitMQClient
{
    public interface IRabbitMqConsumerService
    {
        void StartConsumers(string consumerName, string queueName = null, byte? totalConsumers = null);
        void Dispose();
        event ReceiveMessageDelegate ReceiveMessage;
        event ShutdownDelegate ShutdownConsumer;
        event ReceiveMessageErrorDelegate ReceiveMessageError;
        string ServiceDescription { get; set; }
        byte GetTotalRunningConsumers { get; }
        short GetTotalConsumers { get; }
        IEnumerable<ConsumerInfo> GetConsumerDetails();
        void StopConsumers(string consumerTag = null);
    }
}