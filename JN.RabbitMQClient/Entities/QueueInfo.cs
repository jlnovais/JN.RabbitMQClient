using System;
using System.Collections.Generic;
using System.Text;

namespace JN.RabbitMQClient.Entities
{
    public class QueueInfo
    {
        public uint ConsumerCount { get; set; }
        public uint MessageReadyCount { get; set; }
    }
}
