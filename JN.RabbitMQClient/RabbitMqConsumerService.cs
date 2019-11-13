using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JN.RabbitMQClient.Entities;
using JN.RabbitMQClient.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace JN.RabbitMQClient
{
    public class RabbitMqConsumerService : RabbitMqServiceBase, IRabbitMqConsumerService, IDisposable
    {
        private readonly List<AsyncEventingBasicConsumerExtended> _consumers = new List<AsyncEventingBasicConsumerExtended>();
        private List<IConnection> _connections = new List<IConnection>();
        public event ReceiveMessageDelegate ReceiveMessage;
        public event ShutdownDelegate ShutdownConsumer;
        public event ReceiveMessageErrorDelegate ReceiveMessageError;
        

        private const short MaxChannelsPerConnection = 3;
        private short _totalConsumersToStart;


        public RabbitMqConsumerService(IBrokerConfigConsumers config)
        {
            _config = config;
        }

        public IEnumerable<ConsumerInfo> GetConsumerDetails()
        {
            if (_consumers.Any())
            {
                //use copy of list to avoid error: "Collection was modified; enumeration operation may not execute"
                var consumers = _consumers.Select(x => new ConsumerInfo()
                {
                    Name = x.ConsumerTag,
                    IsRunning = x.IsRunning,
                    ShutdownReason = x.ShutdownReason?.ReplyText ?? "",
                    ConnectedToPort = x.ConnectedToPort,
                    ConnectedToHost = x.ConnectedToHost,
                    ConnectionTime = x.ConnectionTime,
                    LastMessageDate = x.LastMessageDate,
                    Id = x.Id

                });

                return consumers.ToList();

                //return _consumers.Select(x => new ConsumerInfo()
                //{
                //    Name = x.ConsumerTag,
                //    IsRunning = x.IsRunning,
                //    ShutdownReason = x.ShutdownReason?.ReplyText ?? "",
                //    ConnectedToPort = x.ConnectedToPort,
                //    ConnectedToHost = x.ConnectedToHost,
                //    ConnectionTime = x.ConnectionTime,
                //    LastMessageDate = x.LastMessageDate,
                //    Id = x.Id

                //});
            }

            return null;
        }

        public short GetTotalRunningConsumers
        {
            get { return (short) (_consumers.Any() ? _consumers.Count(x => x.IsRunning) : 0); }
        }

        public void StartConsumers(string consumerName, string queueName = null, short? totalConsumers = null)
        {
            _totalConsumersToStart = totalConsumers ?? _config.TotalInstances;

            if (_totalConsumersToStart <= 0)
                throw new ArgumentException("Invalid total number of consumers to start");

            
            for (var i = 0; i < _totalConsumersToStart; i++)
            {
                var totalCreatedConsumers = _consumers.Count;

                var connection = GetConnection(ServiceDescription + "_" + i, totalCreatedConsumers, MaxChannelsPerConnection);

                var channel = connection.CreateModel();
                var consumer = new AsyncEventingBasicConsumerExtended(channel)
                {
                    ConnectedToPort = connection.Endpoint.Port,
                    ConnectedToHost = connection.Endpoint.HostName,
                    ConnectionTime = DateTime.Now,
                    Id = i
                };

                consumer.Received += Consumer_Received;
                consumer.Shutdown += Consumer_Shutdown;

                _consumers.Add(consumer);

                var routingKeyOrQueueName =
                    string.IsNullOrWhiteSpace(queueName) ? _config.RoutingKeyOrQueueName : queueName;

                channel.BasicConsume(routingKeyOrQueueName, false, $"{consumerName}_{i}", consumer);
            }
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
            var consumer = (AsyncEventingBasicConsumer) sender;

            var errorCode = e.ReplyCode;
            var errorMessage = e.ReplyText;
            var consumerTag = consumer.ConsumerTag;
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
                message = Encoding.UTF8.GetString(e.Body);

                var consumer = (AsyncEventingBasicConsumerExtended) sender;

                consumer.LastMessageDate = DateTime.Now;

                var model = consumer.Model; 

                var result = await OnReceiveMessage(routingKeyOrQueueName, consumerTag, exchange, message).ConfigureAwait(false);

                switch (result)
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
                        //TO DO
                        //RequeueMessageWithDelay(deliveryArgs);
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


        protected virtual Task OnMessageReceiveError(string routingKeyOrQueueName, string consumerTag, string exchange, string message, string errorMessage)
        {
            return ReceiveMessageError?.Invoke(routingKeyOrQueueName, consumerTag, exchange, message, errorMessage);
        }

        protected virtual Task<Constants.MessageProcessInstruction> OnReceiveMessage(string routingKeyOrQueueName, string consumerTag, string exchange, string message)
        {
            return ReceiveMessage?.Invoke(routingKeyOrQueueName, consumerTag, exchange, message);
        }

        protected virtual Task OnShutdownConsumer(string consumerTag, ushort errorCode, string shutdownInitiator, string errorMessage)
        {
            return ShutdownConsumer?.Invoke(consumerTag, errorCode, shutdownInitiator, errorMessage);
        }

        public void Dispose()
        {
            if (_consumers.Any())
            {
                foreach (var consumer in _consumers)
                {
                    consumer.Model.Dispose();
                }
            }


            foreach (var connection in _connections)
            {
                connection.Dispose();
            }

        }

    }
}
