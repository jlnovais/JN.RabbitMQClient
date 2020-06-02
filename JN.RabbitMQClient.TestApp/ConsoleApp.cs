﻿using System;
using System.Threading;
using System.Threading.Tasks;
using JN.RabbitMQClient.Entities;
using JN.RabbitMQClient.TestApp.HelperClasses;
using Microsoft.Extensions.Logging;

namespace JN.RabbitMQClient.TestApp
{
    public class ConsoleApp
    {
        private readonly ILogger<ConsoleApp> _logger;
        private readonly IRabbitMqConsumerService _consumerService;
        private readonly IRabbitMqSenderService _senderService;
        private readonly AppConfig _config;

        public ConsoleApp(ILogger<ConsoleApp> logger, IRabbitMqConsumerService consumerService, IRabbitMqSenderService senderService, AppConfig config)
        {
            _logger = logger;

            _consumerService = consumerService;
            _senderService = senderService;
            _config = config;


            _consumerService.ServiceDescription = "Consumer Service";
            _consumerService.ReceiveMessage += ProcessMessage;
            _consumerService.ShutdownConsumer += ProcessShutdown;
            _consumerService.ReceiveMessageError += ProcessError;

            _senderService.ServiceDescription = "Sender Service";

        }


        private static async Task ProcessError(string routingKeyOrQueueName, string consumerTag, string exchange, string message, string errorMessage)
        {
            await Console.Out.WriteLineAsync($"Error processing message: {errorMessage} {Environment.NewLine}Details. routingkeyorqueuename: '{routingKeyOrQueueName}' | consumertag: {consumerTag} | exchange: {exchange} | message: {message}").ConfigureAwait(false);
        }


        // Application starting point
        public void Run()
        {
            var retryQueueDetails = new RetryQueueDetails
            {
                RetentionPeriodInRetryQueueMilliseconds = _config.BrokerRetentionPeriodInRetryQueueSeconds * 1000,
                RetentionPeriodInRetryQueueMillisecondsMax = _config.BrokerRetentionPeriodInRetryQueueSecondsMax * 1000,
                RetryQueue = _config.BrokerRetryQueue
            };

            _consumerService.StartConsumers("consumer", retryQueueDetails);

            //start more consumers, if needed
            _consumerService.StartConsumers("consumer", retryQueueDetails, totalConsumers: 3);

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
            _consumerService.Dispose();
        }

        private async Task<Constants.MessageProcessInstruction> ProcessMessage(string routingKeyOrQueueName, string consumerTag, long firstErrorTimestamp, string exchange, string message)
        {
            var debugMessage = $"Message received by '{consumerTag}'. Exchange: {exchange}. Message: {message} ";

            await Console.Out.WriteLineAsync(debugMessage).ConfigureAwait(false);

            _logger.LogInformation(debugMessage);

            var details = _consumerService.GetConsumerDetails();

            if (details != null)
            {
                foreach (var consumerInfo in details)
                {
                    Console.ForegroundColor = consumerInfo.Id % 2 == 0 ? ConsoleColor.Blue : ConsoleColor.DarkGreen;

                    await Console.Out.WriteLineAsync($"Consumer '{consumerInfo.Name}'; connected to {consumerInfo.ConnectedToHost}:{consumerInfo.ConnectedToPort}; firstErrorTimestamp: {firstErrorTimestamp}; started at {consumerInfo.ConnectionTime:yyyy-MM-dd HH:mm:ss}; {consumerInfo.LastMessageDate:yyyy-MM-dd HH:mm:ss} ").ConfigureAwait(false);

                    Console.ResetColor();
                }
            }

            switch (message)
            {
                case "ok":
                    _senderService.Send(message);
                    await Console.Out.WriteLineAsync($"Message sent !! ").ConfigureAwait(false);
                    return Constants.MessageProcessInstruction.OK;
                case "ignore":
                    return Constants.MessageProcessInstruction.IgnoreMessage;
                case "requeue":
                    return Constants.MessageProcessInstruction.IgnoreMessageWithRequeue;
                case "delay":
                    return Constants.MessageProcessInstruction.RequeueMessageWithDelay;
                case "error":
                    throw new ErrorProcessingException("error processing message");
                default:
                    return Constants.MessageProcessInstruction.Unknown;
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