using System;
using System.Collections.Generic;
using System.Text;
using JN.RabbitMQClient.Interfaces;
using RabbitMQ.Client;

namespace JN.RabbitMQClient.Entities
{
    public class MessageProperties : IMessageProperties
    {
        public string AppId { get; set; }
        public string ContentEncoding { get; set; }
        public string ContentType { get; set; }
        public string CorrelationId { get; set; }
        public byte DeliveryMode { get; set; }
        public string Expiration { get; set; }
        public IDictionary<string, object> Headers { get; set; }
        public string MessageId { get; set; }
        public bool Persistent { get; set; }
        public byte Priority { get; set; }
        public string ReplyTo { get; set; }
        public string Type { get; set; }
        public string UserId { get; set; }
    }
}
