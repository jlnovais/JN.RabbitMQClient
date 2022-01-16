﻿using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;

namespace JN.RabbitMQClient.Other
{
    internal static class RabbitMqUtilities
    {
        private static readonly Random Random = new Random();

    
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


        internal static string GetAdditionalInfoFromMessageArgs(IBasicProperties properties)
        {
            if (properties?.Headers == null)
                return null;

            if (!properties.Headers.ContainsKey(Constants.MessageAdditionalInfoHeaderName)) 
                return null;

            if (properties.Headers[Constants.MessageAdditionalInfoHeaderName] == null) 
                return null;
            
            var value = (byte[]) properties.Headers[Constants.MessageAdditionalInfoHeaderName];
            var res = Encoding.UTF8.GetString(value);
            
            return res;
        }

        internal static QueueDeclareOk CreateQueueOrGetInfo(string queueName, IModel channel)
        {
            return channel.QueueDeclare(queueName, true, false, false);
        }


        internal static void SetPropertiesSenderRequeueMessageWithDelay(IBasicProperties properties, int retentionPeriodInRetryQueueMilliseconds, int retentionPeriodInRetryQueueMillisecondsMax)
        {
            properties.Persistent = true; 
            properties.Timestamp = new AmqpTimestamp(DateTime.UtcNow.ToUnixTimestamp());
            properties.Expiration = GetNumber(retentionPeriodInRetryQueueMilliseconds, retentionPeriodInRetryQueueMillisecondsMax).ToString();
            properties.Headers = new Dictionary<string, object>();
        }

        internal static void SetPropertiesSender(IBasicProperties properties, string encoding)
        {
            properties.Persistent = true; 
            properties.Timestamp = new AmqpTimestamp(DateTime.UtcNow.ToUnixTimestamp());
            properties.ContentEncoding = encoding;
        }

        private static int GetNumber(int min, int max)
        {
            return min>=max ? min : Random.Next(min, max + 1);
        }
    }
}
