using System;
using System.Threading.Tasks;
using JN.RabbitMQClient.Entities;
using JN.RabbitMQClient.Interfaces;

namespace JN.RabbitMQClient.SimpleConsoleTestApp
{
    public static class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            // consumer

            var consumerService = new RabbitMqConsumerService(GetBrokerConfigConsumers());

            consumerService.ReceiveMessage += ReceiveMessage;
            consumerService.ShutdownConsumer += ShutdownConsumer;
            consumerService.ReceiveMessageError += ReceiveMessageError;

            consumerService.StartConsumers("my consumer");

            // sender

            var senderService = new RabbitMqSenderService(GetBrokerConfigSender());

            senderService.Send("my message");

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();

            consumerService.Dispose();
        }

        private static IBrokerConfigSender GetBrokerConfigSender()
        {
            IBrokerConfigSender configSender = new BrokerConfigSender
            {
                Username = "test",
                Password = "123",
                Host = "localhost",
                VirtualHost = "/",
                RoutingKeyOrQueueName = "MyTestQueue"
            };
            return configSender;
        }

        private static IBrokerConfigConsumers GetBrokerConfigConsumers()
        {
            IBrokerConfigConsumers configConsumers = new BrokerConfigConsumers
            {
                Username = "test",
                Password = "123",
                Host = "localhost",
                VirtualHost = "/",
                RoutingKeyOrQueueName = "MyTestQueue",
                ShuffleHostList = false,
                Port = 0,
                TotalInstances = 3
            };
            return configConsumers;
        }

        private static async Task ReceiveMessageError(string routingKeyOrQueueName, string consumerTag, string exchange, string message, string errorMessage)
        {
            await Console.Out.WriteLineAsync($"Error: '{consumerTag}' | Queued message: {message} | Error message: {errorMessage}").ConfigureAwait(false);
        }

        private static async Task ShutdownConsumer(string consumerTag, ushort errorCode, string shutdownInitiator, string errorMessage)
        {
            await Console.Out.WriteLineAsync($"Shutdown '{consumerTag}' | {errorCode} | {shutdownInitiator} | {errorMessage}").ConfigureAwait(false);
        }

        private static async Task<Constants.MessageProcessInstruction> ReceiveMessage(string routingKeyOrQueueName, string consumerTag, long firstErrorTimestamp, string exchange, string message)
        {
            await Console.Out.WriteLineAsync($"Message received from '{consumerTag}' ({exchange}): {message} ").ConfigureAwait(false);
            return Constants.MessageProcessInstruction.OK;
        }
    }
}
