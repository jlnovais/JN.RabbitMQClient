using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JN.RabbitMQClient.Entities;
using JN.RabbitMQClient.Interfaces;
using JN.RabbitMQClient.Limiter;
using JN.RabbitMQClient.Other;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace JN.RabbitMQClient
{
    /// <summary>
    /// Service for consuming messages.
    /// </summary>
    public class RabbitMqConsumerService : RabbitMqServiceBase, IRabbitMqConsumerService
    {
        private readonly List<AsyncEventingBasicConsumerExtended> _consumers = new List<AsyncEventingBasicConsumerExtended>();
        private List<IConnection> _connections = new List<IConnection>();
        
        /// <summary>
        /// Delegate executed when a message is received
        /// </summary>
        public event ReceiveMessageDelegate ReceiveMessage;
        /// <summary>
        /// Delegate to execute when consumer is shut down
        /// </summary>
        public event ShutdownDelegate ShutdownConsumer;
        /// <summary>
        /// Delegate to execute when an error occurs
        /// </summary>
        public event ReceiveMessageErrorDelegate ReceiveMessageError;

        /// <summary>
        /// Number of channels per connection
        /// </summary>
        public byte MaxChannelsPerConnection { get; set; } = 3;

        private byte _prefetch = 1;
        /// <summary>
        /// Message prefetch (default is 1) for each consumer
        /// </summary>
        public byte ConsumersPrefetch
        {
            get => _prefetch;
            set => _prefetch = value < 1 ? (byte)1 : value;
        }

        private bool _disposedValue;
        
        public RabbitMqConsumerService(IBrokerConfigConsumers config)
        {
            _config = config;
        }


        public ILimiter Limiter { get; set; }


        public IEnumerable<ConsumerInfo> GetConsumerDetails()
        {
            if (!_consumers.Any())
                return null;

            //use copy of list to avoid error: "Collection was modified; enumeration operation may not execute"
            var consumers = _consumers.Select(x => new ConsumerInfo
            {
                Name = string.Join(";", x.ConsumerTags),
                IsRunning = x.IsRunning,
                ShutdownReason = x.ShutdownReason?.ReplyText ?? "",
                ConnectedToPort = x.ConnectedToPort,
                ConnectedToHost = x.ConnectedToHost,
                ConnectionTime = x.ConnectionTime,
                LastMessageDate = x.LastMessageDate,
                Id = x.Id

            });

            return consumers.ToList();
        }

        public byte GetTotalRunningConsumers
        {
            get { return (byte)(_consumers.Any() ? _consumers.Count(x => x.IsRunning) : 0); }
        }

        public short GetTotalConsumers => (short)(_consumers.Any() ? _consumers.Count : 0);


        /// <summary>
        /// StartConsumers - start consumers and connect them to a queue.
        /// </summary>
        /// <param name="consumerName">Consumer name</param>
        /// <param name="queueName">Queue where the consumers will connect (optional - if not defined, the config value is used)</param>
        /// <param name="totalConsumers">Total consumers to start (optional - if not defined, the config value is used)</param>
        /// <param name="createQueue">Create queue to connect when starting consumers (optional - default is false)</param>
        public void StartConsumers(string consumerName, string queueName = null, byte? totalConsumers = null, bool createQueue = false)
        {
            StartConsumers(consumerName, null, queueName, totalConsumers, createQueue);
        }

        /// <summary>
        /// StartConsumers - start consumers and connect them to a queue.
        /// </summary>
        /// <param name="consumerName">Consumer name</param>
        /// <param name="retryQueueDetails">Retry queue details if a message needs to be requeued with a delay (a Dead letter exchange must be defined)</param>
        /// <param name="queueName">Queue where the consumers will connect (optional - if not defined, the config value is used)</param>
        /// <param name="totalConsumers">Total consumers to start (optional - if not defined, the config value is used)</param>
        /// <param name="createQueue">Create queue to connect when starting consumers (optional - default is false)</param>
        /// <exception cref="RabbitMQClientException"></exception>
        public void StartConsumers(string consumerName, RetryQueueDetails retryQueueDetails, string queueName = null, byte? totalConsumers = null, bool createQueue = false)
        {
            var config = (IBrokerConfigConsumers) _config;

            var totalConsumersToStart = totalConsumers ?? config.TotalInstances;

            if (totalConsumersToStart <= 0)
                throw new ArgumentException("Invalid total number of consumers to start");

            var triedCreateQueue = false;

            for (var i = 0; i < totalConsumersToStart; i++)
            {
                var routingKeyOrQueueName =
                    string.IsNullOrWhiteSpace(queueName) ? config.RoutingKeyOrQueueName : queueName;

                var totalCreatedConsumers = _consumers.Count;

                var connection = GetConnection(ServiceDescription + "_" + i, totalCreatedConsumers, MaxChannelsPerConnection);

                var channel = connection.CreateModel();

                channel.BasicQos(prefetchCount: _prefetch, prefetchSize:0, global: false);

                if (createQueue && !triedCreateQueue)
                {
                    try
                    {
                        RabbitMqUtilities.CreateQueueOrGetInfo(routingKeyOrQueueName, channel);
                        triedCreateQueue = true;
                    }
                    catch (Exception e)
                    {
                        throw new RabbitMqClientException($"Unable do create queue. Details: {e.Message}", e);
                    }
                }

                var consumer = new AsyncEventingBasicConsumerExtended(channel)
                {
                    ConnectedToPort = connection.Endpoint.Port,
                    ConnectedToHost = connection.Endpoint.HostName,
                    ConnectionTime = DateTime.Now,
                    Id = i,
                    RetentionPeriodInRetryQueueMilliseconds = retryQueueDetails?.RetentionPeriodInRetryQueueMilliseconds ?? 0,
                    RetentionPeriodInRetryQueueMillisecondsMax = retryQueueDetails?.RetentionPeriodInRetryQueueMillisecondsMax ?? 0,
                    RetryQueue = retryQueueDetails?.RetryQueue
                };

                consumer.Received += Consumer_Received;
                consumer.Shutdown += Consumer_Shutdown;

                var tag = $"{consumerName}_{i}";

                consumer.ConsumerTag = tag;
                
                _consumers.Add(consumer);

                channel.BasicConsume(routingKeyOrQueueName, false, tag, consumer);
            }
        }


        private async Task Consumer_Shutdown(object sender, ShutdownEventArgs e)
        {
            var consumer = (AsyncEventingBasicConsumerExtended)sender;

            var errorCode = e.ReplyCode;
            var errorMessage = e.ReplyText;

            //on shutdown consumer.ConsumerTags (from base class) is empty; can't use it here

            var consumerTag = consumer.ConsumerTag;
            var shutdownInitiator = e.Initiator.ToString();

            await OnShutdownConsumer(consumerTag, errorCode, shutdownInitiator, errorMessage).ConfigureAwait(false);
        }



        /// <summary>
        /// Stop consumers
        /// </summary>
        public void StopConsumers()
        {
            StopConsumers(null);
        }

        /// <summary>
        /// Stop consumers
        /// </summary>
        /// <param name="consumerTag">Consumer tag (optional). If specified, all consumers with tag starting with this value will be stopped" </param>
        public void StopConsumers(string consumerTag)
        {
            if (!string.IsNullOrWhiteSpace(consumerTag))
            {
                foreach (var consumer in _consumers.Where(x => x.ConsumerTag.StartsWith(consumerTag)))
                {
                    consumer.Model.Abort();
                    consumer.Model.Dispose();
                }

                return;
            }

            foreach (var consumer in _consumers)
            {
                consumer?.Model.Abort();
                consumer?.Model.Dispose();
            }
        }


 


        private IConnection GetConnection(string connectionName, int totalConsumers = 0, short maxChannelsPerConnection = 1)
        {

            if (maxChannelsPerConnection == 0)
                throw new ArgumentException("MaxChannelsPerConnection can't be 0");

            var m = totalConsumers / maxChannelsPerConnection;

            CleanConnections();

            if (_connections.Count <= m)
            {
                var conn = base.GetConnection(connectionName);
                _connections.Add(conn);
                return conn;
            }

            return _connections.Last();
        }

        private void CleanConnections()
        {
            if (_connections.Any())
                _connections = _connections.FindAll(conn =>
                {
                    var isOpen = conn.IsOpen;

                    if (!isOpen)
                        conn.Dispose();

                    return isOpen;
                });
        }


        private async Task Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            var consumerTag = e.ConsumerTag;
            var routingKeyOrQueueName = e.RoutingKey;
            var exchange = e.Exchange;
            var message = "";
            IModel model = null;

            try
            {
                var properties = e.BasicProperties;

                message = GetEncoding(properties).GetString(e.Body.Span.ToArray());

                var consumer = (AsyncEventingBasicConsumerExtended)sender;

                consumer.LastMessageDate = DateTime.Now;

                var firstErrorTimestamp = RabbitMqUtilities.GetFirstErrorTimeStampFromMessageArgs(e.BasicProperties);
                var additionalInfo = properties.CorrelationId;
                model = consumer.Model;

                var messageProcessInstruction = await GetMessageProcessInstruction(routingKeyOrQueueName, consumerTag, firstErrorTimestamp, exchange, message, additionalInfo, properties.ToProperties() ).ConfigureAwait(false);
                    
                switch (messageProcessInstruction.Value)
                {
                    case Constants.MessageProcessInstruction.OK:
                        model.BasicAck(e.DeliveryTag, false);
                        break;
                    case Constants.MessageProcessInstruction.IgnoreMessage:
                        model.BasicReject(e.DeliveryTag, false);
                        break;
                    case Constants.MessageProcessInstruction.IgnoreMessageWithRequeue:
                        model.BasicReject(e.DeliveryTag, true);
                        break;
                    case Constants.MessageProcessInstruction.RequeueMessageWithDelay:
                        RequeueMessageWithDelay(consumer, e, messageProcessInstruction);
                        model.BasicReject(e.DeliveryTag, false);
                        break;
                    default:
                        model.BasicAck(e.DeliveryTag, false);
                        break;
                }

            }
            catch (Exception ex)
            {
                await OnMessageReceiveError(routingKeyOrQueueName, consumerTag, exchange, message, ex.Message)
                    .ConfigureAwait(false);

                model?.BasicReject(e.DeliveryTag, false);
            }
        }

        private static Encoding GetEncoding(IBasicProperties properties)
        {
            try
            {
                var encoding = string.IsNullOrWhiteSpace(properties.ContentEncoding)
                    ? Encoding.UTF8
                    : Encoding.GetEncoding(properties.ContentEncoding);
                return encoding;
            }
            catch
            {
                return Encoding.UTF8;
            }
        }

        private async Task<MessageProcessInstruction> GetMessageProcessInstruction(string routingKeyOrQueueName,
            string consumerTag,
            long firstErrorTimestamp, string exchange, string message, string messageAdditionalInfo,
            IMessageProperties properties)
        {
            var isAllowed = true;

            if (Limiter != null)
                isAllowed = Limiter.IsAllowed(routingKeyOrQueueName, consumerTag, firstErrorTimestamp, exchange, message);

            if (!isAllowed)
                return new MessageProcessInstruction(Limiter.DeniedProcessInstruction, messageAdditionalInfo);

            var messageProcessInstruction =
                await OnReceiveMessage(routingKeyOrQueueName, consumerTag, firstErrorTimestamp, exchange, message, messageAdditionalInfo, properties)
                    .ConfigureAwait(false);
            
            return messageProcessInstruction;
        }

        private void RequeueMessageWithDelay(AsyncEventingBasicConsumerExtended consumer, BasicDeliverEventArgs deliveryArgs, MessageProcessInstruction messageProcessInstruction)
        {

            if (string.IsNullOrWhiteSpace(consumer.RetryQueue))
                return;

            var properties = deliveryArgs.BasicProperties; 
            RabbitMqUtilities.SetPropertiesSenderRequeueMessageWithDelay(properties, consumer.RetentionPeriodInRetryQueueMilliseconds, consumer.RetentionPeriodInRetryQueueMillisecondsMax, messageProcessInstruction.Priority);

            var firstErrorTimeStamp = RabbitMqUtilities.GetFirstErrorTimeStampFromMessageArgs(deliveryArgs.BasicProperties);
            SetFirstErrorTimeStampToProperties(firstErrorTimeStamp, properties);

            properties.CorrelationId = messageProcessInstruction.AdditionalInfo;

            consumer.Model.BasicPublish(
                "",
                consumer.RetryQueue,
                properties,
                deliveryArgs.Body);
        }

        private static void SetFirstErrorTimeStampToProperties(long firstErrorTimeStamp, IBasicProperties properties)
        {
            if (properties.Headers is null)
                properties.Headers = new ConcurrentDictionary<string, object>();

            if (properties.Headers.ContainsKey(Constants.FirstErrorTimeStampHeaderName))
                return;

            properties.Headers. Add(Constants.FirstErrorTimeStampHeaderName,
                firstErrorTimeStamp > 0 ? firstErrorTimeStamp : DateTime.UtcNow.ToUnixTimestamp());
        }


        protected virtual Task OnMessageReceiveError(string routingKeyOrQueueName, string consumerTag, string exchange, string message, string errorMessage)
        {
            return ReceiveMessageError?.Invoke(routingKeyOrQueueName, consumerTag, exchange, message, errorMessage);
        }

        protected virtual Task<MessageProcessInstruction> OnReceiveMessage(string routingKeyOrQueueName, string consumerTag, long firstErrorTimestamp, string exchange, string message, string messageAdditionalInfo, IMessageProperties properties)
        {
            return ReceiveMessage?.Invoke(routingKeyOrQueueName, consumerTag, firstErrorTimestamp, exchange, message, messageAdditionalInfo, properties);
        }

        protected virtual Task OnShutdownConsumer(string consumerTag, ushort errorCode, string shutdownInitiator, string errorMessage)
        {
            return ShutdownConsumer?.Invoke(consumerTag, errorCode, shutdownInitiator, errorMessage);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposedValue) 
                return;

            if (disposing)
            {
                foreach (var connection in _connections)
                {
                    connection.Abort();
                    connection.Dispose();
                }
            }

            _disposedValue = true;
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
