using JN.RabbitMQClient.Interfaces;
using System;

namespace JN.RabbitMQClient.Other
{
    public static class Utilities
    {
        public static long GetStreamOffsetFromMessageHeader(IMessageProperties properties)
        {
            if (properties?.Headers == null)
            {
                return 0;
            }

            if (properties.Headers.TryGetValue(Constants.StreamOffsetHeaderName, out var value))
                return (long)value;

            return 0;
        }

    }
}