using System.Threading.Tasks;

namespace JN.RabbitMQClient
{
    public delegate Task<Constants.MessageProcessInstruction> ReceiveMessageDelegate(string routingKeyOrQueueName, string consumerTag, string exchange, string message);

    public delegate Task ShutdownDelegate(string consumerTag, ushort errorCode, string shutdownInitiator, string errorMessage);

    public delegate Task ReceiveMessageErrorDelegate(string routingKeyOrQueueName, string consumerTag, string exchange, string message, string errorMessage);


}
