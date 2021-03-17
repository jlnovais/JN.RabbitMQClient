namespace JN.RabbitMQClient.Interfaces
{
    public interface IRabbitMqSenderServiceKeepConnection: IRabbitMqSenderService
    {
        void SetupConnection();
        bool IsConnected { get; }
    }
}