# JN.RabbitMQClient

[![Codacy Badge](https://api.codacy.com/project/badge/Grade/259943d527ba4afc872e2b922ea52321)](https://app.codacy.com/manual/jlnovais/JN.RabbitMQClient?utm_source=github.com&utm_medium=referral&utm_content=jlnovais/JN.RabbitMQClient&utm_campaign=Badge_Grade_Dashboard)

Simple implementation of RabbitMQ consumer and sender.

This version IS NOT compatible with version 1.x.x.

## Install
Download the package from NuGet:

`Install-Package JN.RabbitMQClient`

## Usage
First, you must create the `RabbitMqConsumerService` and then define delegates for `ReceiveMessage`, `ShutdownConsumer` and `ReceiveMessageError`. The service will start the required number of consumers when `StartConsumers` is called. 

To use a retry queue, the method `StartConsumers` should be called with a `RetryQueueDetails` object. 

## Message processing instructions
`OK` - message is considered as successfully processed
`RequeueMessageWithDelay` - message is removed from the queue, but sent to a retry queue for latter processing (typically with a dead letter configuration)
`IgnoreMessage` - message is removed from the queue and ignored
`IgnoreMessageWithRequeue` - message is rejected and sent back to the queue

## Example

Example for consumer and sender services:

```csharp
    class Program
    {
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
            IBrokerConfigSender configSender = new BrokerConfigSender()
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
            IBrokerConfigConsumers configConsumers = new BrokerConfigConsumers()
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
    }

```
