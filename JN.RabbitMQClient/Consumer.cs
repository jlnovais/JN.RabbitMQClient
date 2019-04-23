using System;
using System.Text;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQClient.Interfaces;

namespace JN.RabbitMQClient
{
    public class Consumer : RabbitMQClientBase, IConsumer
    {


        private bool _started;

        public string Description { get; set; }

        public ushort PrefetchCount { get; set; }

        private bool _isConsuming = false;
        public bool IsConsuming {
            get
            {
                try
                {
                    _isConsuming = _consumer.IsRunning;
                }
                catch 
                {
                    _isConsuming = false;
                }
                
                return _isConsuming;
            }

            //private set => _isConsuming = value;
        }

        public string LastError { get; private set; }
        public DateTime? LastErrorDate { get; private set; }

        public string RetryQueue { get; set; }
        public int RetentionPeriodInRetryQueueMilliseconds { get; set; }
        public int ErrorCode { get; private set; }

        public DateTime? LastMessageReceivedDate { get; private set; } = null;

        public DateTime StartDate { get; private set; }

        public event MessageReceiveDelegate onMessageReceived;
        public event StopReceiveDelegate onStopReceive;

        private EventingBasicConsumer _consumer;

        public bool AutomaticConnectionRecovery { get; set; }


        //public new string ConnectedServer => base.ConnectedServer;

        internal void OnStopReceiveHandler(string queuename, string lastErrorDescription, string consumerDescription)
        {
            StopReceiveDelegate handler = onStopReceive;
            if (handler != null)
                handler(queuename, lastErrorDescription, consumerDescription);
        }

        internal Constants.MessageProcessInstruction OnMessageReceivedHandler(string message, string sourceQueueName, long firstErrorTimestamp, string consumerDescription)
        {
            MessageReceiveDelegate handler = onMessageReceived;
            if (handler != null)
                return handler(message, sourceQueueName, firstErrorTimestamp, consumerDescription);

            return Constants.MessageProcessInstruction.Unknown;
        }

        //internal delegate to run the consuming queue on a seperate thread
        private delegate void ConsumeDelegate();

        public Consumer()
        {
            //The .net client has a dedicated worker thread that listens to a TCP socket and pulls the messages off as they arrive and places them on a shared thread-safe queue (in memory queue inside the .net client). 
            //The client application, pulls messages off the shared queue on its own thread and processes them as required.
            //prefectchCount = maximum number of messages that the .net client queue will hold at any one time
            //Setting the prefectchCount to a reasonably high number will allow RabbitMQ to more efficiently stream messages across the network.
            PrefetchCount = 10;
        }



        public void Stop()
        {
           //if (IsConsuming)
                Dispose();

            //IsConsuming = false;
            
        }

        private void Consume()
        {
            Thread.CurrentThread.IsBackground = false;

            _consumer = new EventingBasicConsumer(_model);

            _consumer.Received -= ConsumerOnReceived;
            _consumer.Shutdown -= Consumer_Shutdown;

            _consumer.Received += ConsumerOnReceived;
            _consumer.Shutdown += Consumer_Shutdown;

            try
            {
                _model.BasicConsume(QueueName, false, _consumer);
            }
            catch (Exception ex)
            {
                UpdateExceptionDetails(_consumer, ex);
            }


        }

        private void Consumer_Shutdown(object sender, ShutdownEventArgs e)
        {
            //IsConsuming = false;

            if (e != null)
            {
                var details = "";

                if (e.Cause != null)
                {
                    details = ((System.Exception) e.Cause).Message;
                }

                LastError = e.ReplyText + "; Code: " + e.ReplyCode + "; Details: " + details;
                LastErrorDate = DateTime.Now;
                ErrorCode = e.ReplyCode;

            }

            OnStopReceiveHandler(QueueName, LastError, Description);

        }

        private void ConsumerOnReceived(object sender, BasicDeliverEventArgs deliveryArgs)
        {
            LastMessageReceivedDate = DateTime.Now;

            var firstErrorTimestamp = Utils.GetFirstErrorTimeStampFromMessageArgs(deliveryArgs.BasicProperties);

            var message = Encoding.UTF8.GetString(deliveryArgs.Body);

            var messageProcessInstruction = OnMessageReceivedHandler(message, QueueName, firstErrorTimestamp, Description);

            switch (messageProcessInstruction)
            {
                case Constants.MessageProcessInstruction.OK:
                    _model.BasicAck(deliveryArgs.DeliveryTag, false);
                    break;
                case Constants.MessageProcessInstruction.IgnoreMessage:
                    _model.BasicReject(deliveryArgs.DeliveryTag, false);
                    break;
                case Constants.MessageProcessInstruction.IgnoreMessageWithRequeue:
                    _model.BasicReject(deliveryArgs.DeliveryTag, true);
                    break;
                case Constants.MessageProcessInstruction.RequeueMessageWithDelay:
                    RequeueMessageWithDelay(deliveryArgs);
                    _model.BasicReject(deliveryArgs.DeliveryTag, false);
                    break;
                default:
                    _model.BasicAck(deliveryArgs.DeliveryTag, false);
                    break;
            }

            Thread.Sleep(1);
        }


        private void UpdateExceptionDetails(EventingBasicConsumer consumer, Exception ex)
        {
            var errorMsg = "";

            try
            {
                var reason = consumer.Model.CloseReason;
                if (reason != null)
                {
                    errorMsg = reason.ReplyText + "; Code: " + reason.ReplyCode + "; ";
                    ErrorCode = reason.ReplyCode;
                }
            }
            catch
            {
                // ignored
            }

            var type = ex.GetType();
            LastError = $"{(string.IsNullOrWhiteSpace(errorMsg) ? ex.Message : $"{errorMsg}; {ex.Message}")}; {type}";
            LastErrorDate = DateTime.Now;

        }


        private void RequeueMessageWithDelay(BasicDeliverEventArgs deliveryArgs)
        {

            if (string.IsNullOrWhiteSpace(RetryQueue))
                return;

            var properties = _model.CreateBasicProperties();
            Tools.SetPropertiesConsumer(properties, RetentionPeriodInRetryQueueMilliseconds);

            var firstErrorTimeStamp = Utils.GetFirstErrorTimeStampFromMessageArgs(deliveryArgs.BasicProperties);
            SetFirstErrorTimeStampToProperties(firstErrorTimeStamp, properties);

            _model.BasicPublish(
                "",
                RetryQueue,
                properties,
                deliveryArgs.Body);
        }


        private void SetFirstErrorTimeStampToProperties(long firstErrorTimeStamp, IBasicProperties properties)
        {
            properties.Headers.Add(Constants.FirstErrorTimeStampHeaderName,
                firstErrorTimeStamp > 0 ? firstErrorTimeStamp : DateTime.UtcNow.ToUnixTimestamp());
        }

        public void Start(string connectionDescription)
        {
            StartDate = DateTime.Now;
            
            if (IsConsuming || _started)
                throw new Exception("Consumer already running. Queue: " + QueueName);

            _started = true;

            LastError = "";

            SetupConnection(connectionDescription, false, AutomaticConnectionRecovery);

            // BasicQos(0="Dont send me a new message until I’ve finished",  PrefetchCount= "Send me PrefetchCount messages at a time", false ="Apply to this Model only")
            _model.BasicQos(0, PrefetchCount, false);

            //IsConsuming = true;

            ConsumeDelegate c = Consume;
            c.BeginInvoke(null, null);
        }

        public void Start()
        {
            Start(Description);
        }

    }

    
}