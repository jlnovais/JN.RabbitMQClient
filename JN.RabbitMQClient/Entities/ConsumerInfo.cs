using System;

namespace JN.RabbitMQClient.Entities
{
    public class ConsumerInfo
    {
        public string Name { get; set; }
        public bool IsRunning { get; set; }
        public string ShutdownReason { get; set; }
        public int ConnectedToPort { get; set; }
        public string ConnectedToHost { get; set; }
        public DateTime ConnectionTime { get; set; }
        public DateTime LastMessageDate { get; set; }
        public int Id { get; set; }
    }
}