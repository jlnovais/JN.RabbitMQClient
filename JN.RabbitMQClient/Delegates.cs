namespace JN.RabbitMQClient
{
    public delegate Constants.MessageProcessInstruction MessageReceiveDelegate(
        string message, string sourceQueueName, long firstErrorTimestamp, string consumerDescription);

    public delegate void StopReceiveDelegate(string queueName, string lastErrorDescription, string consumerDescription);
}