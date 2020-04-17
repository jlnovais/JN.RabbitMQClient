using System;
using System.Collections.Generic;
using System.Text;

namespace JN.RabbitMQClient.Entities
{
    public class RabbitMqClientException: Exception
    {
        public RabbitMqClientException(string message):base(message)
        {
        }

        public RabbitMqClientException(string message, Exception e) : base(message, e)
        {
        }
    }
}
