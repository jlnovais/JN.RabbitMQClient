using System.Collections.Generic;

namespace JN.RabbitMQClient.Interfaces
{
    public interface IMessageProperties
    {
        string AppId { get; set; }
        string ContentEncoding { get; set; }
        string ContentType { get; set; }
        string CorrelationId { get; set; }
        byte DeliveryMode { get; set; }
        string Expiration { get; set; }
        IDictionary<string, object> Headers { get; set; }
        string MessageId { get; set; }
        bool Persistent { get; set; }
        byte Priority { get; set; }
        string ReplyTo { get; set; }
        string Type { get; set; }
        string UserId { get; set; }
    }
}