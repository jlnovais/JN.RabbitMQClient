using System;
using System.Collections.Generic;
using JN.RabbitMQClient.Entities;
using JN.RabbitMQClient.Limiter;

namespace JN.RabbitMQClient.Interfaces
{
    public interface IRabbitMqConsumerService: IDisposable
    {
        /// <summary>
        /// StartConsumers - start consumers and connect them to a queue.
        /// </summary>
        /// <param name="consumerName">Consumer name</param>
        /// <param name="queueName">Queue where the consumers will connect (optional - if not defined, the config value is used)</param>
        /// <param name="totalConsumers">Total consumers to start (optional - if not defined, the config value is used)</param>
        /// <param name="createQueue">Create queue to connect when starting consumers (optional - default is false)</param>
        void StartConsumers(string consumerName, string queueName = null, byte? totalConsumers = null,
            bool createQueue = false);

        /// <summary>
        /// StartConsumers - start consumers and connect them to a queue.
        /// </summary>
        /// <param name="consumerName">Consumer name.</param>
        /// <param name="retryQueueDetails">Retry queue details if a message needs to be requeued with a delay (a Dead letter exchange must be defined)</param>
        /// <param name="queueName">Queue where the consumers will connect (optional - if not defined, the config value is used)</param>
        /// <param name="totalConsumers">Total consumers to start (optional - if not defined, the config value is used)</param>
        /// <param name="createQueue">Create queue to connect when starting consumers (optional - default is false)</param>
        void StartConsumers(string consumerName, RetryQueueDetails retryQueueDetails, string queueName = null,
            byte? totalConsumers = null, bool createQueue = false);

        /// <summary>
        /// Stop consumers
        /// </summary>
        void StopConsumers();

        /// <summary>
        /// Stop consumers
        /// </summary>
        /// <param name="consumerTag">Consumer tag (optional). If specified, it must be the complete tag. Tag = consumerName (specified in StartConsumers method ) + "_" + id; Example : "consumerTest_0" </param>
        void StopConsumers(string consumerTag);

        /// <summary>
        /// Event executed when a message is received.
        /// </summary>
        event ReceiveMessageDelegate ReceiveMessage;

        /// <summary>
        /// Event executed when the consumer shuts down.
        /// </summary>
        event ShutdownDelegate ShutdownConsumer;

        /// <summary>
        /// Event executed when an error occurs.
        /// </summary>
        event ReceiveMessageErrorDelegate ReceiveMessageError;
        string ServiceDescription { get; set; }
        byte TotalRunningConsumers { get; }
        short TotalConsumers { get; }
        ILimiter Limiter { get; set; }

        /// <summary>
        /// Number of channels per connection
        /// </summary>
        byte MaxChannelsPerConnection { get; set; }

        /// <summary>
        /// Message prefetch (default is 1) for each consumer
        /// </summary>
        byte ConsumersPrefetch { get; set; }

        /// <summary>
        /// Get consumer service connection details
        /// </summary>
        ConnectionDetails ConnectionDetails { get; }

        IEnumerable<ConsumerInfo> GetConsumerDetails();



    }
}