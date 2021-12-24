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
            this.Value = value;
            this.AdditionalInfo = additionalInfo;
        }
        
        public Constants.MessageProcessInstruction Value { get; set; }
        public string AdditionalInfo { get; set; }
        
    }
}
