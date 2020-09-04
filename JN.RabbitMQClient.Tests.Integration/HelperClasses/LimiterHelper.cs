using System;
using System.Collections.Generic;
using System.Text;
using JN.RabbitMQClient.Limiter;

namespace JN.RabbitMQClient.Tests.Integration.HelperClasses
{
    public class LimiterHelper: ILimiter
    {
        public int TotalCalls { get; set; }
        public LimiterHelper(Constants.MessageProcessInstruction deniedProcessInstruction)
        {
            DeniedProcessInstruction = deniedProcessInstruction;
            TotalCalls = 0;
        }

        public Constants.MessageProcessInstruction DeniedProcessInstruction { get; }

        public bool IsAllowed(string routingKeyOrQueueName, string consumerTag, long firstErrorTimestamp, string exchange,
            string message)
        {
            TotalCalls++;

            return true;
        }
    }
}
