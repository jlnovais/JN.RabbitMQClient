using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;

namespace JN.RabbitMQClient.Other
{
    internal class Tools
    {
        internal static long GetFirstErrorTimeStampFromMessageArgs(IBasicProperties properties)
        {
            long res = 0;

            if (properties == null)
                return res;

            if (properties.Headers == null)
                return res;

            if (properties.Headers.ContainsKey(Constants.FirstErrorTimeStampHeaderName))
                res = (long)(properties.Headers[Constants.FirstErrorTimeStampHeaderName]);

            return res;
        }


        internal static void SetPropertiesConsumer(IBasicProperties properties, int retentionPeriodInRetryQueueMilliseconds)
        {
            properties.Persistent = true; // SetPersistent(true);
            properties.Timestamp = new AmqpTimestamp(DateTime.UtcNow.ToUnixTimestamp());
            properties.Expiration = retentionPeriodInRetryQueueMilliseconds.ToString();
            properties.Headers = new Dictionary<string, object>();
        }

        internal static void SetPropertiesSender(IBasicProperties properties, string encoding)
        {
            properties.Persistent = true; // SetPersistent(true);
            properties.Timestamp = new AmqpTimestamp(DateTime.UtcNow.ToUnixTimestamp());
            properties.ContentEncoding = encoding;
        }
    }
}
