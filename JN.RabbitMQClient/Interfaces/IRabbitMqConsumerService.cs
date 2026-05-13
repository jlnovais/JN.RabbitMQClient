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
        /// <param name="consumerName">Consumer name</param>
        /// <param name="retryQueueDetails">Retry queue details if a message needs to be requeued with a delay (a Dead letter exchange must be defined)</param>
        /// <param name="queueName">Queue where the consumers will connect (optional - if not defined, the config value is used)</param>
        /// <param name="totalConsumers">Total consumers to start (optional - if not defined, the config value is used)</param>
        /// <param name="createQueue">Create queue to connect when starting consumers (optional - default is false)</param>
        /// <param name="queueIsStream">When creating a queue, specify if is a stream (optional - default is false)</param>
        /// <param name="streamOffset">Specifies the stream offset when connecting to a stream (has no effect if connecting to a queue). Available options:
        /// <list type="bullet">
        /// <item>
        ///     <term>"next"</term>
        ///     <description> (string) The default that listens on new messages</description>
        ///</item>
        /// <item>
        ///     <term>"first" </term>
        ///     <description>(string) The first message available in the Stream segments</description>
        ///</item>
        /// <item>
        ///     <term>"last"</term>
        ///     <description>(string) All messages starting from the last written Chunk to the Segment file.</description>
        ///     </item>
        ///<item>
        ///     <term>offset</term>
        ///     <description>(number)  Specify a starting Index where to start reading from. Ex: <example>5000</example></description>
        ///</item>
        /// <item>
        ///     <term>timestamp</term>
        ///     <description>(string)  The Posix timestamp to start reading from</description>
        ///</item>
        /// <item>
        ///     <term>interval</term>
        ///     <description>(string)  A string that defines how long back from CURRENT TIME to start reading, same string format as the x-max-age argument when creating the stream. Valid units are Y, M, D, h, m, s. Ex: <example>"1h"</example> (one hour), <example>"30s"</example> (30 seconds), <example>"1D"</example> (one day), <example>"1m"</example> (one minute).</description>
        ///</item>
        /// </list>
        /// </param>
        void StartConsumers(string consumerName, RetryQueueDetails retryQueueDetails, string queueName = null,
            byte? totalConsumers = null, bool createQueue = false, bool queueIsStream = false,
            object streamOffset = null);

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