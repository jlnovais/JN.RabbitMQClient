using System;
using System.Collections.Generic;
using System.Text;

namespace JN.RabbitMQClient.TestApp.HelperClasses
{
    public class ErrorProcessingException:Exception
    {
        public ErrorProcessingException(string message): base(message)
        {
        }
    }
}
