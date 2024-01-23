using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using JN.RabbitMQClient.Entities;
using JN.RabbitMQClient.Interfaces;
using JN.RabbitMQClient.Other;

namespace JN.RabbitMQClient.TestApp.HelperClasses
{
    internal static class MessageHelper
    {
        internal static bool HasExpired(long firstErrorTimestamp, long brokerMessageTTLSeconds)
        {
            if (firstErrorTimestamp == 0)
                return false;

            var elapsedTime = (DateTime.UtcNow.ToUnixTimestamp() - firstErrorTimestamp);

            return elapsedTime > brokerMessageTTLSeconds;
        }

        internal static async Task SendTestMessage(this IRabbitMqSenderService senderService, string message)
        {
            var msgProperties = new MessageProperties()
            {
                Headers = new Dictionary<string, object>()
                {
                    { "key", "vale" },
                    { "key2", "value2" }
                },

                Persistent = true,
                //ContentEncoding = System.Text.Encoding.UTF8.HeaderName,
                //Priority = 8,
                CorrelationId = "123456ABC"
            };

            var res = senderService.Send(message, msgProperties);

            if (res.Success)
            {
                await Console.Out.WriteLineAsync($"Message sent successfully !! ").ConfigureAwait(false);

                if (res.ReturnedObject != null)
                {
                    await Console.Out.WriteLineAsync($"Consumer count: {res.ReturnedObject.ConsumerCount}").ConfigureAwait(false);
                    await Console.Out.WriteLineAsync($"Message count: {res.ReturnedObject.MessageReadyCount}").ConfigureAwait(false);
                }
            }
               



        }

        internal static async Task ShowConsumerDetailsOnConsole(long firstErrorTimestamp, IEnumerable<ConsumerInfo> details)
        {
            if (details != null)
            {
                foreach (var consumerInfo in details)
                {
                    Console.ForegroundColor = consumerInfo.Id % 2 == 0 ? ConsoleColor.Blue : ConsoleColor.DarkGreen;

                    await Console.Out.WriteLineAsync(
                            $"Consumer '{consumerInfo.Name}'; connected to {consumerInfo.ConnectedToHost}:{consumerInfo.ConnectedToPort}; firstErrorTimestamp: {firstErrorTimestamp}; started at {consumerInfo.ConnectionTime:yyyy-MM-dd HH:mm:ss}; {consumerInfo.LastMessageDate:yyyy-MM-dd HH:mm:ss} ")
                        .ConfigureAwait(false);

                    Console.ResetColor();
                }
            }
        }
    }
}
