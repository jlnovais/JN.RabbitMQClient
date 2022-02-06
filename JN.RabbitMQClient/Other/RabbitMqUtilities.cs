using System;
using System.Collections.Generic;
using System.Text;
using JN.RabbitMQClient.Interfaces;
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


        //internal static string GetAdditionalInfoFromMessageArgs(IBasicProperties properties)
        //{
        //    if (properties?.Headers == null)
        //        return null;

        //    if (!properties.Headers.ContainsKey(Constants.MessageAdditionalInfoHeaderName)) 
        //        return null;

        //    if (properties.Headers[Constants.MessageAdditionalInfoHeaderName] == null) 
        //        return null;
            
        //    var value = (byte[]) properties.Headers[Constants.MessageAdditionalInfoHeaderName];
        //    var res = Encoding.UTF8.GetString(value);
            
        //    return res;
        //}

        internal static QueueDeclareOk CreateQueueOrGetInfo(string queueName, IModel channel)
        {
            return channel.QueueDeclare(queueName, true, false, false);
        }


        internal static void SetPropertiesSenderRequeueMessageWithDelay(IBasicProperties properties, int retentionPeriodInRetryQueueMilliseconds, int retentionPeriodInRetryQueueMillisecondsMax)
        {
            //properties.Persistent = true; 
            properties.Timestamp = new AmqpTimestamp(DateTime.UtcNow.ToUnixTimestamp());
            properties.Expiration = GetNumber(retentionPeriodInRetryQueueMilliseconds, retentionPeriodInRetryQueueMillisecondsMax).ToString();
            //properties.Headers = new Dictionary<string, object>();
        }

        internal static void SetProperties(IBasicProperties properties, IMessageProperties msgProperties)
        {
            properties.Timestamp = new AmqpTimestamp(DateTime.UtcNow.ToUnixTimestamp());

            if (!string.IsNullOrWhiteSpace(msgProperties.AppId))
                properties.AppId = msgProperties.AppId;

            if (!string.IsNullOrWhiteSpace(msgProperties.ContentEncoding))
                properties.ContentEncoding = msgProperties.ContentEncoding;

            if (!string.IsNullOrWhiteSpace(msgProperties.ContentEncoding))
                properties.ContentType = msgProperties.ContentType;

            if (!string.IsNullOrWhiteSpace(msgProperties.CorrelationId))
                properties.CorrelationId = msgProperties.CorrelationId;

            if (msgProperties.DeliveryMode > 0)
                properties.DeliveryMode = msgProperties.DeliveryMode;

            if (!string.IsNullOrWhiteSpace(msgProperties.Expiration))
                properties.Expiration = msgProperties.Expiration;

            if (!(msgProperties.Headers is null))
                properties.Headers = msgProperties.Headers;

            if (!string.IsNullOrWhiteSpace(msgProperties.MessageId))
                properties.MessageId = msgProperties.MessageId;

            if (!string.IsNullOrWhiteSpace(msgProperties.MessageId))
                properties.MessageId = msgProperties.MessageId;

            properties.Persistent = msgProperties.Persistent;

            if (msgProperties.Priority > 0)
                properties.Priority = msgProperties.Priority;

            if (!string.IsNullOrWhiteSpace(msgProperties.ReplyTo))
                properties.ReplyTo = msgProperties.ReplyTo;

            if (!string.IsNullOrWhiteSpace(msgProperties.Type))
                properties.Type = msgProperties.Type;

            if (!string.IsNullOrWhiteSpace(msgProperties.UserId))
                properties.Type = msgProperties.UserId;
        }

        private static int GetNumber(int min, int max)
        {
            return min>=max ? min : Random.Next(min, max + 1);
        }
    }
}
