using System.Collections.Generic;
using JN.RabbitMQClient.Entities;

namespace JN.RabbitMQClient
{
    public interface IRabbitMqConsumerService
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
        /// <param name="consumerTag">Consumer tag (optional). If specified, it must be the complete tag. Tag = consumerName (specified in StartConsumers method ) + "_" + id; Example : "consumerTest_0" </param>
        void StopConsumers(string consumerTag = null);


        void Dispose();
        event ReceiveMessageDelegate ReceiveMessage;
        event ShutdownDelegate ShutdownConsumer;
        event ReceiveMessageErrorDelegate ReceiveMessageError;
        string ServiceDescription { get; set; }
        byte GetTotalRunningConsumers { get; }
        short GetTotalConsumers { get; }
        IEnumerable<ConsumerInfo> GetConsumerDetails();



    }
}