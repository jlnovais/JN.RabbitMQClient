using System.Collections.Generic;
using JN.RabbitMQClient.Entities;

namespace JN.RabbitMQClient
{
    public interface IRabbitMqConsumerService
    {
        void StartConsumers(string consumerName, string queueName = null, short? totalConsumers = null);
        void Dispose();
        event ReceiveMessageDelegate ReceiveMessage;
        event ShutdownDelegate ShutdownConsumer;
        event ReceiveMessageErrorDelegate ReceiveMessageError;
        string ServiceDescription { get; set; }
        short GetTotalRunningConsumers { get; }
        IEnumerable<ConsumerInfo> GetConsumerDetails();
    }
}