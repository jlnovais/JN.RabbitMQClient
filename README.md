# JN.RabbitMQClient
Simple implementation of RabbitMQ consumer and sender.

This version IS NOT compatible with previous versions.


## Install
Download the package from NuGet:

`Install-Package JN.RabbitMQClient`

## Usage
First, you must create the `RabbitMqConsumerService` and then define delegates for `ReceiveMessage`, `ShutdownConsumer` and `ReceiveMessageError`. The service will start the required number of consumers when `StartConsumers` is called.

Example for consumer and sender services:

```csharp
        static void Main(string[] args)
        {
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
            IBrokerConfigSender configSender = new BrokerConfig()
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
            IBrokerConfigConsumers configConsumers = new BrokerConfig()
            {
                Username = "test",
                Password = "123",
                Host = "localhost",
                VirtualHost = "/",
                RoutingKeyOrQueueName = "MyTestQueue",
                ShuffleHostList = false,
                Exchange = "",
                Port = 0,
                TotalInstances = 3
            };
            return configConsumers;
        }

        private static async Task ReceiveMessageError(string routingKeyOrQueueName, string consumerTag, string exchange, string message, string errorMessage)
        {
            await Console.Out.WriteLineAsync($"Error: '{consumerTag}' | {errorMessage}");
        }

        private static async Task ShutdownConsumer(string consumerTag, ushort errorCode, string shutdownInitiator, string errorMessage)
        {
            await Console.Out.WriteLineAsync($"Shutdown '{consumerTag}' | {errorCode} | {shutdownInitiator} | {errorMessage}");
        }

        private static async Task<Constants.MessageProcessInstruction> ReceiveMessage(string routingKeyOrQueueName, string consumerTag, long firstErrorTimestamp, string exchange, string message)
        {
            await Console.Out.WriteLineAsync($"Message received from '{consumerTag}': {message}");
            return Constants.MessageProcessInstruction.OK;
        }

```







