using System.Threading.Tasks;
using JN.RabbitMQClient.Entities;
using JN.RabbitMQClient.Interfaces;

namespace JN.RabbitMQClient
{
    public delegate Task<MessageProcessInstruction> ReceiveMessageDelegate(string routingKeyOrQueueName, string consumerTag, long firstErrorTimestamp, string exchange, string message, string messageAdditionalInfo, IMessageProperties properties);

    public delegate Task ShutdownDelegate(string consumerTag, ushort errorCode, string shutdownInitiator, string errorMessage);

    public delegate Task ReceiveMessageErrorDelegate(string routingKeyOrQueueName, string consumerTag, string exchange, string message, string errorMessage);
}
