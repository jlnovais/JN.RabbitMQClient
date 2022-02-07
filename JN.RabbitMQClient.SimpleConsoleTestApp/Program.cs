using System;
using System.Threading.Tasks;
using JN.RabbitMQClient.Entities;
using JN.RabbitMQClient.Interfaces;

namespace JN.RabbitMQClient.SimpleConsoleTestApp
{
    public static class Program
    {
        private static readonly string hostName = "localhost";
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            // consumer

            var consumerService = new RabbitMqConsumerService(GetBrokerConfigConsumers());

            consumerService.ReceiveMessage += ReceiveMessage;
            consumerService.ShutdownConsumer += ShutdownConsumer;
            consumerService.ReceiveMessageError += ReceiveMessageError;
            consumerService.MaxChannelsPerConnection = 5;
            consumerService.ServiceDescription = "test consumer service";

            try
            {
                consumerService.StartConsumers("my consumer");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error starting consumers: {e.Message}");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                Environment.Exit(-1);
            }
            

            // sender

            var senderService = new RabbitMqSenderService(GetBrokerConfigSender());

            IMessageProperties properties = new MessageProperties { Priority = 3 };

            senderService.Send("my message", properties);

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
                Host = hostName,
                VirtualHost = "MyVirtualHost",
                RoutingKeyOrQueueName = "MyTestQueue",
                KeepConnectionOpen = true
            };
            return configSender;
        }

        private static IBrokerConfigConsumers GetBrokerConfigConsumers()
        {
            IBrokerConfigConsumers configConsumers = new BrokerConfigConsumers
            {
                Username = "test",
                Password = "123",
                Host = hostName,
                VirtualHost = "MyVirtualHost",
                RoutingKeyOrQueueName = "MyTestQueue",
                ShuffleHostList = false,
                Port = 0,
                TotalInstances = 100
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

        private static async Task<MessageProcessInstruction> ReceiveMessage(string routingKeyOrQueueName, string consumerTag, long firstErrorTimestamp, string exchange, string message, string additionalInfo, IMessageProperties properties)
        {
            var priorityReceived = properties.Priority;

            var newPriority = (byte)(priorityReceived <= 3 ? 5 : priorityReceived);

            await Console.Out.WriteLineAsync($"Message received by '{consumerTag}' from queue '{routingKeyOrQueueName}': {message}; Priority received: {properties.Priority} ").ConfigureAwait(false);
            
            return new MessageProcessInstruction
            {
                Value = Constants.MessageProcessInstruction.OK,
                Priority = newPriority,
                AdditionalInfo = "id: 123"
            };
        }
    }
}
