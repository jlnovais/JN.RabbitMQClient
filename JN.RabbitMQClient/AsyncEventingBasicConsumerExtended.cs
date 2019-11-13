﻿using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace JN.RabbitMQClient
{
    internal class AsyncEventingBasicConsumerExtended : AsyncEventingBasicConsumer
    {
        public int ConnectedToPort { get; set; }
        public string ConnectedToHost { get; set; }

        public DateTime ConnectionTime { get; set; }
        public DateTime LastMessageDate { get; set; }
        public int Id { get; set; }

        public AsyncEventingBasicConsumerExtended(IModel model) : base(model)
        {
        }


    }
}