using System;
using System.Collections.Generic;
using RabbitMQ.Client;

namespace JN.RabbitMQClient
{
    internal static class Tools
    {
        internal static void SetPropertiesSender(IBasicProperties properties)
        {
            properties.Persistent = true; // SetPersistent(true);
            properties.Timestamp = new AmqpTimestamp(DateTime.UtcNow.ToUnixTimestamp());
        }

        internal static void SetPropertiesConsumer(IBasicProperties properties, int retentionPeriodInRetryQueueMilliseconds)
        {
            properties.Persistent = true; // SetPersistent(true);
            properties.Timestamp = new AmqpTimestamp(DateTime.UtcNow.ToUnixTimestamp());
            properties.Expiration = retentionPeriodInRetryQueueMilliseconds.ToString();
            properties.Headers = new Dictionary<string, object>();
        }
    }
}
