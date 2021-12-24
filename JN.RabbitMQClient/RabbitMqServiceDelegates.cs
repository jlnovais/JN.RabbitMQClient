using System.Threading.Tasks;
using JN.RabbitMQClient.Entities;

namespace JN.RabbitMQClient
{
    public delegate Task<MessageProcessInstruction> ReceiveMessageDelegate(string routingKeyOrQueueName, string consumerTag, long firstErrorTimestamp, string exchange, string message, string messageAdditionalInfo);

    public delegate Task ShutdownDelegate(string consumerTag, ushort errorCode, string shutdownInitiator, string errorMessage);

    public delegate Task ReceiveMessageErrorDelegate(string routingKeyOrQueueName, string consumerTag, string exchange, string message, string errorMessage);
}
