# JN.RabbitMQClient

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT) [![Nuget](https://img.shields.io/nuget/v/JN.RabbitMQClient)](https://www.nuget.org/packages/JN.RabbitMQClient/) [![Nuget](https://img.shields.io/nuget/vpre/JN.RabbitMQClient)](https://www.nuget.org/packages/JN.RabbitMQClient/) [![Build Status](https://travis-ci.org/jlnovais/JN.RabbitMQClient.svg?branch=master)](https://travis-ci.org/jlnovais/JN.RabbitMQClient) ![.NET Core](https://github.com/jlnovais/JN.RabbitMQClient/workflows/.NET%20Core/badge.svg) [![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=jlnovais_JN.RabbitMQClient&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=jlnovais_JN.RabbitMQClient) [![CodeQL](https://github.com/jlnovais/JN.RabbitMQClient/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/jlnovais/JN.RabbitMQClient/actions/workflows/codeql-analysis.yml) [![Compile & Test .NET](https://github.com/jlnovais/JN.RabbitMQClient/actions/workflows/dotnetcore.yml/badge.svg)](https://github.com/jlnovais/JN.RabbitMQClient/actions/workflows/dotnetcore.yml)

Simple implementation of RabbitMQ consumer and sender.

**Features**

*   Sender implementation
*   Multiple consumer instances supported
*   Multiple processing options for received messages
*   Random expiration for messages sent to an holding queue (depending on the processing option)
*   TLS connection support 
*   [Limiter for message processing](https://jn-rabbitmqclient.josenovais.com/#limiter)
*   Message properties for more advanced scenarios such as queues with support for priority messages, messages Headers, etc.

More details available on the [project website](https://jn-rabbitmqclient.josenovais.com/).

## Install
Download the package from NuGet:

`Install-Package JN.RabbitMQClient -Version [version number]`

## Usage
First, you must create the `RabbitMqConsumerService` and then define delegates for `ReceiveMessage`, `ShutdownConsumer` and `ReceiveMessageError`. The service will start the required number of consumers when `StartConsumers` is called. 

To use a retry queue, the method `StartConsumers` should be called with a `RetryQueueDetails` object. 

## Message processing instructions
`OK` - message is considered as successfully processed

`RequeueMessageWithDelay` - message is removed from the queue, but sent to a holding queue for later processing (typically with a dead letter configuration)

`IgnoreMessage` - message is removed from the queue and ignored

`IgnoreMessageWithRequeue` - message is rejected and sent back to the queue

## Example

Example for consumer and sender services:

```csharp
    class Program
    {
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

            consumerService.StartConsumers("my consumer");
 
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
                TotalInstances = 4
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

```
