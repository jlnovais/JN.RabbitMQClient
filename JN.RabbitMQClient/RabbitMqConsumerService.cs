using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JN.RabbitMQClient.Entities;
using JN.RabbitMQClient.Interfaces;
using JN.RabbitMQClient.Other;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace JN.RabbitMQClient
{
    public class RabbitMqConsumerService : RabbitMqServiceBase, IRabbitMqConsumerService
    {
        private readonly List<AsyncEventingBasicConsumerExtended> _consumers = new List<AsyncEventingBasicConsumerExtended>();
        private List<IConnection> _connections = new List<IConnection>();
        public event ReceiveMessageDelegate ReceiveMessage;
        public event ShutdownDelegate ShutdownConsumer;
        public event ReceiveMessageErrorDelegate ReceiveMessageError;


        private const short MaxChannelsPerConnection = 3;
        private bool _disposed;


        public RabbitMqConsumerService(IBrokerConfigConsumers config)
        {
            _config = config;
        }

        public IEnumerable<ConsumerInfo> GetConsumerDetails()
        {
            if (!_consumers.Any())
                return null;

            //use copy of list to avoid error: "Collection was modified; enumeration operation may not execute"
            var consumers = _consumers.Select(x => new ConsumerInfo
            {
                //2020-05-04
                //Name = x.ConsumerTag,
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

                _consumers.Add(consumer);

                channel.BasicConsume(routingKeyOrQueueName, false, $"{consumerName}_{i}", consumer);
            }
        }

        /// <summary>
        /// Stop consumers
        /// </summary>
        /// <param name="consumerTag">Consumer tag (optional). If specified, it must be the complete tag. Tag = consumerName (specified in StartConsumers method ) + "_" + id; Example : "consumerTest_0" </param>
        public void StopConsumers()
        {
            StopConsumers(null);
        }
        public void StopConsumers(string consumerTag)
        {
            if (!string.IsNullOrWhiteSpace(consumerTag))
            {
                //2020-05-04
                //var consumer = _consumers.First(x => x.ConsumerTag == consumerTag);
                var consumer = _consumers.First(x => x.ConsumerTags.Contains(consumerTag));
                consumer?.Model.Abort();

                _consumers.Remove(consumer);

                return;
            }

            foreach (var consumer in _consumers)
            {
                consumer.Model.Abort();
            }

            _consumers.RemoveAll(x => !x.IsRunning);
        }

        private IConnection GetConnection(string connectionName, int totalConsumers = 0, short maxChannelsPerConnection = 1)
        {

            var m = totalConsumers / maxChannelsPerConnection;

            CleanConnections();

            if (_connections.Count() <= m)
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


        private async Task Consumer_Shutdown(object sender, ShutdownEventArgs e)
        {
            var consumer = (AsyncEventingBasicConsumer)sender;

            var errorCode = e.ReplyCode;
            var errorMessage = e.ReplyText;
            //2020-05-04
            //var consumerTag = consumer.ConsumerTag;
            var consumerTag =  string.Join(";", consumer.ConsumerTags);
            var shutdownInitiator = e.Initiator.ToString();

            await OnShutdownConsumer(consumerTag, errorCode, shutdownInitiator, errorMessage).ConfigureAwait(false);
        }



        private async Task Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            var consumerTag = e.ConsumerTag;
            var routingKeyOrQueueName = e.RoutingKey;
            var exchange = e.Exchange;
            var message = "";

        
            try
            {
 
                message = Encoding.UTF8.GetString(e.Body.Span);

                var consumer = (AsyncEventingBasicConsumerExtended)sender;

                consumer.LastMessageDate = DateTime.Now;

                var firstErrorTimestamp = RabbitMqUtilities.GetFirstErrorTimeStampFromMessageArgs(e.BasicProperties);

                var model = consumer.Model;

                var messageProcessInstruction = await OnReceiveMessage(routingKeyOrQueueName, consumerTag, firstErrorTimestamp, exchange, message).ConfigureAwait(false);

                switch (messageProcessInstruction)
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
                        RequeueMessageWithDelay(consumer, e);
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
            }
        }

        private void RequeueMessageWithDelay(AsyncEventingBasicConsumerExtended consumer, BasicDeliverEventArgs deliveryArgs)
        {

            if (string.IsNullOrWhiteSpace(consumer.RetryQueue))
                return;

            var properties = consumer.Model.CreateBasicProperties();
            RabbitMqUtilities.SetPropertiesConsumer(properties, consumer.RetentionPeriodInRetryQueueMilliseconds, consumer.RetentionPeriodInRetryQueueMillisecondsMax);

            var firstErrorTimeStamp = RabbitMqUtilities.GetFirstErrorTimeStampFromMessageArgs(deliveryArgs.BasicProperties);
            SetFirstErrorTimeStampToProperties(firstErrorTimeStamp, properties);

            consumer.Model.BasicPublish(
                "",
                consumer.RetryQueue,
                properties,
                deliveryArgs.Body);
        }

        private static void SetFirstErrorTimeStampToProperties(long firstErrorTimeStamp, IBasicProperties properties)
        {
            properties.Headers.Add(Constants.FirstErrorTimeStampHeaderName,
                firstErrorTimeStamp > 0 ? firstErrorTimeStamp : DateTime.UtcNow.ToUnixTimestamp());
        }


        protected virtual Task OnMessageReceiveError(string routingKeyOrQueueName, string consumerTag, string exchange, string message, string errorMessage)
        {
            return ReceiveMessageError?.Invoke(routingKeyOrQueueName, consumerTag, exchange, message, errorMessage);
        }

        protected virtual Task<Constants.MessageProcessInstruction> OnReceiveMessage(string routingKeyOrQueueName, string consumerTag, long firstErrorTimestamp, string exchange, string message)
        {
            return ReceiveMessage?.Invoke(routingKeyOrQueueName, consumerTag, firstErrorTimestamp, exchange, message);
        }

        protected virtual Task OnShutdownConsumer(string consumerTag, ushort errorCode, string shutdownInitiator, string errorMessage)
        {
            return ShutdownConsumer?.Invoke(consumerTag, errorCode, shutdownInitiator, errorMessage);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            if (_consumers.Any())
            {
                foreach (var consumer in _consumers)
                {
                    try
                    {
                        consumer.Model.Close();
                    }
                    catch (Exception)
                    {
                        // ignored
                    }

                    consumer.Model.Dispose();
                }
                _consumers.Clear();
            }


            foreach (var connection in _connections)
            {
                connection.Abort();
                connection.Dispose();
            }

            _disposed = true;
        }

    }
}
