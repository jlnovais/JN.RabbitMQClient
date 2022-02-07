using System;
using System.Collections.Generic;
using System.Text;

namespace JN.RabbitMQClient.Entities
{
    public class MessageProcessInstruction
    {
        public MessageProcessInstruction()
        {
        }

        public MessageProcessInstruction(Constants.MessageProcessInstruction value, string additionalInfo = null)
        {
            Priority = 0;
            Value = value;
            AdditionalInfo = additionalInfo;
        }

        public MessageProcessInstruction(Constants.MessageProcessInstruction value, string additionalInfo, byte priority)
        {
            Priority = priority;
            Value = value;
            AdditionalInfo = additionalInfo;
        }
        
        public Constants.MessageProcessInstruction Value { get; set; }
        public string AdditionalInfo { get; set; }
        public byte Priority { get; set; }

    }
}
