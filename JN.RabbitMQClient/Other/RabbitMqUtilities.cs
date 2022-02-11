using System;
using JN.RabbitMQClient.Entities;
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


        internal static QueueDeclareOk CreateQueueOrGetInfo(string queueName, IModel channel)
        {
            return channel.QueueDeclare(queueName, true, false, false);
        }


        internal static void SetPropertiesSenderRequeueMessageWithDelay(IBasicProperties properties, int retentionPeriodInRetryQueueMilliseconds, int retentionPeriodInRetryQueueMillisecondsMax, byte? priority)
        {
            properties.Timestamp = new AmqpTimestamp(DateTime.UtcNow.ToUnixTimestamp());
            properties.Expiration = GetNumber(retentionPeriodInRetryQueueMilliseconds, retentionPeriodInRetryQueueMillisecondsMax).ToString();

            if (priority.HasValue)
                properties.Priority = priority.Value;
        }


        internal static IMessageProperties ToProperties(this IBasicProperties properties)
        {
            if (properties is null)
                return null;

            var msgProperties = new MessageProperties
            {
                AppId = properties.AppId,
                ContentEncoding = properties.ContentEncoding,
                ContentType = properties.ContentType,
                CorrelationId = properties.CorrelationId,
                DeliveryMode = properties.DeliveryMode,
                Expiration = properties.Expiration,
                Headers = properties.Headers,
                MessageId = properties.MessageId,
                Persistent = properties.Persistent,
                Priority = properties.Priority,
                ReplyTo = properties.ReplyTo,
                Type = properties.Type,
                UserId = properties.UserId
            };

            return msgProperties;
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

            if (msgProperties.DeliveryMode.HasValue)
                properties.DeliveryMode = msgProperties.DeliveryMode.Value;

            if (!string.IsNullOrWhiteSpace(msgProperties.Expiration))
                properties.Expiration = msgProperties.Expiration;

            if (!(msgProperties.Headers is null))
                properties.Headers = msgProperties.Headers;

            if (!string.IsNullOrWhiteSpace(msgProperties.MessageId))
                properties.MessageId = msgProperties.MessageId;

            if (!string.IsNullOrWhiteSpace(msgProperties.MessageId))
                properties.MessageId = msgProperties.MessageId;

            if (msgProperties.Persistent.HasValue)
                properties.Persistent = msgProperties.Persistent.Value;

            if (msgProperties.Priority.HasValue)
                properties.Priority = msgProperties.Priority.Value;

            if (!string.IsNullOrWhiteSpace(msgProperties.ReplyTo))
                properties.ReplyTo = msgProperties.ReplyTo;

            if (!string.IsNullOrWhiteSpace(msgProperties.Type))
                properties.Type = msgProperties.Type;

            if (!string.IsNullOrWhiteSpace(msgProperties.UserId))
                properties.UserId = msgProperties.UserId;
        }

        private static int GetNumber(int min, int max)
        {
            return min >= max ? min : Random.Next(min, max + 1);
        }
    }
}
