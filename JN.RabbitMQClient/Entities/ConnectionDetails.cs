using System;
using System.Collections.Generic;
using System.Text;

namespace JN.RabbitMQClient.Entities
{
    public class ConnectionDetails
    {
        public string Host { get; set; }
        public string Username { get; set; }
        public string VirtualHost { get; set; }
        public int Port { get; set; }
        public bool UseTLS { get; set; }
        public string RoutingKeyOrQueueName { get; set; }
        public string Exchange { get; set; }
    }
}
