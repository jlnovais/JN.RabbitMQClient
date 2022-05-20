using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using JN.RabbitMQClient.Entities;
using JN.RabbitMQClient.Interfaces;
using JN.RabbitMQClient.Limiter;
using JN.RabbitMQClient.TestApp.HelperClasses;
using Microsoft.Extensions.Logging;
using JN.RabbitMQClient.Other;
using Microsoft.Extensions.Configuration;

namespace JN.RabbitMQClient.TestApp
{
    public class ConsoleApp
    {
        private readonly ILogger<ConsoleApp> _logger;
        private readonly IRabbitMqConsumerService _consumerService;
        private readonly IRabbitMqSenderService _senderService;
        private readonly IConfiguration _configuration;

        private readonly BrokerConfigConsumersRetry _retryConfig;


        public ConsoleApp(ILogger<ConsoleApp> logger, IRabbitMqConsumerService consumerService, IRabbitMqSenderService senderService, ILimiter limiter, IConfiguration configuration)
        {
            _logger = logger;

            _consumerService = consumerService;
            _senderService = senderService;
            _configuration = configuration;
            _retryConfig = configuration.GetBrokerConfigConsumersRetry("BrokerConfigConsumersRetry");
            
            _consumerService.ServiceDescription = "Consumer Service";
            _consumerService.ReceiveMessage += ProcessMessage;
            _consumerService.ShutdownConsumer += ProcessShutdown;
            _consumerService.ReceiveMessageError += ProcessError;

            _consumerService.Limiter = limiter;
            _consumerService.MaxChannelsPerConnection = 3;
            _consumerService.ConsumersPrefetch = 2;
            _senderService.ServiceDescription = "Sender Service";
        }


        private static async Task ProcessError(string routingKeyOrQueueName, string consumerTag, string exchange, string message, string errorMessage)
        {
            await Console.Out.WriteLineAsync($"Error processing message: {errorMessage} {Environment.NewLine}Details. routingKeyOrQueueName: '{routingKeyOrQueueName}' | consumerTag: {consumerTag} | exchange: {exchange} | message: {message} | Error: {errorMessage}").ConfigureAwait(false);
        }

        // Application starting point
        public void Run()
        {
            _senderService.ServiceDescription = $"service to send messages - {DateTime.Now}";

            var retryQueueDetails = new RetryQueueDetails
            {
                RetentionPeriodInRetryQueueMilliseconds = _retryConfig.BrokerRetentionPeriodInRetryQueueSeconds * 1000,
                RetentionPeriodInRetryQueueMillisecondsMax = _retryConfig.BrokerRetentionPeriodInRetryQueueSecondsMax * 1000,
                RetryQueue = _retryConfig.BrokerRetryQueue
            };

            
            
            _consumerService.StartConsumers("consumers_Tag_A", retryQueueDetails, totalConsumers: 2);
            _consumerService.StartConsumers("consumers_Tag_B", retryQueueDetails, totalConsumers: 2, queueName: _configuration.GetString("OtherQueueName"));

            _logger.LogInformation($"Starting consumers...");

            var details = _consumerService.GetConsumerDetails();

            if (details != null)
            {
                foreach (var consumerInfo in details)
                {
                    _logger.LogInformation($"consumer '{consumerInfo.Name}' connected to {consumerInfo.ConnectedToHost}:{consumerInfo.ConnectedToPort} started at {consumerInfo.ConnectionTime:yyyy-MM-dd HH:mm:ss}");
                }

            }

        }

        public void Stop()
        {
            _consumerService.StopConsumers("consumers_Tag_A");
            _consumerService.StopConsumers("consumers_Tag_B");

        }

 
        
        private async Task<MessageProcessInstruction> ProcessMessage(string routingKeyOrQueueName, string consumerTag, long firstErrorTimestamp, string exchange, string message, string additionalInfo, IMessageProperties properties)
        {
            var priorityReceived = properties.Priority;

            var debugMessage = $"Message received by '{consumerTag}' from {routingKeyOrQueueName}. Exchange: {exchange}. Message: {message}. Priority: {priorityReceived}. Additional info: {additionalInfo} ";

            await Console.Out.WriteLineAsync(debugMessage).ConfigureAwait(false);

            _logger.LogInformation(debugMessage);

            if (MessageHelper.HasExpired(firstErrorTimestamp, _retryConfig.BrokerMessageTTLSeconds))
            {
                await Console.Out.WriteLineAsync("Message expired.").ConfigureAwait(false);
                return new MessageProcessInstruction(Constants.MessageProcessInstruction.IgnoreMessage);
            }
            
            var details = _consumerService.GetConsumerDetails();

            await MessageHelper.ShowConsumerDetailsOnConsole(firstErrorTimestamp, details);

            switch (message)
            {
                case "ok":
                    await _senderService.SendTestMessage(message);

                    return new MessageProcessInstruction(Constants.MessageProcessInstruction.OK);
                case "ignore":
                    return new MessageProcessInstruction(Constants.MessageProcessInstruction.IgnoreMessage);
                case "requeue":
                    return new MessageProcessInstruction(Constants.MessageProcessInstruction.IgnoreMessageWithRequeue);
                case "delay":
                    
                    var newPriority = (byte)(priorityReceived <= 3 ? 5 : priorityReceived);

                    return new MessageProcessInstruction
                    {
                        Value = Constants.MessageProcessInstruction.RequeueMessageWithDelay,
                        AdditionalInfo = $"message delayed {DateTime.Now}",
                        Priority = newPriority
                    };
                case "error":
                    throw new ErrorProcessingException("error processing message");
                default:
                    return new MessageProcessInstruction(Constants.MessageProcessInstruction.Unknown);
            }
        }

        private async Task ProcessShutdown(string consumerTag, ushort errorCode, string shutdownInitiator, string errorMessage)
        {
            var debugMessage = $"Shutdown '{consumerTag}' | {errorCode} | {shutdownInitiator} | {errorMessage} {Environment.NewLine} TotalConsumers: {_consumerService.GetTotalConsumers} | TotalRunningConsumers: {_consumerService.GetTotalRunningConsumers} ";
            await Console.Out.WriteLineAsync(debugMessage).ConfigureAwait(false);
            _logger.LogInformation(debugMessage);
        }
    }
}