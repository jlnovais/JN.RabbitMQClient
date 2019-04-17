using System;
using JN.RabbitMQClient;

namespace RabbitMQClient.Interfaces
{
    public interface IConsumer : IDisposable
    {
        event MessageReceiveDelegate onMessageReceived;
        event StopReceiveDelegate onStopReceive;
        bool IsConsuming { get; }
        void Stop();
        void Start();

        void Start(string connectionDescription);


        string Description { get; set; }

        ushort PrefetchCount { get; set; }

        string LastError { get; }
        DateTime? LastErrorDate { get; }

        string RetryQueue { get; set; }
        int RetentionPeriodInRetryQueueMilliseconds { get; set; }
        int ErrorCode { get; }

        string ConnectedServer { get; }

        DateTime? LastMessageReceivedDate { get; }

        DateTime StartDate { get; }
        bool AutomaticConnectionRecovery { get; set; }

        string Username { get; set; }
        string Password { get; set; }		
        string Host { get; set; }
        string QueueName { get; set; }
        string VirtualHost { get; set; }
        int Port { get; set; }
        int ConnectionTimeoutMillisecs { get; set; }
        bool ShuffleHostList { get; set; }


    }
}