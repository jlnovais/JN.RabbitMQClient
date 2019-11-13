﻿using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace JN.RabbitMQClient.TestApp
{
    public class ConsoleApp
    {
        private readonly ILogger<ConsoleApp> _logger;
        private readonly IRabbitMqConsumerService _consumerService;
        private readonly IRabbitMqSenderService _senderService;

        public ConsoleApp(ILogger<ConsoleApp> logger, IRabbitMqConsumerService consumerService, IRabbitMqSenderService senderService)
        {
            _logger = logger;
 
            _consumerService = consumerService;
            _senderService = senderService;


            _consumerService.ServiceDescription = "Consumer Service";
            _consumerService.ReceiveMessage += ProcessMessage;
            _consumerService.ShutdownConsumer += ProcessShutdown;
            _consumerService.ReceiveMessageError += ProcessError;

            _senderService.ServiceDescription = "Sender Service";

        }

        private async Task ProcessError(string routingkeyorqueuename, string consumertag, string exchange, string message, string errormessage)
        {
            await Console.Out.WriteLineAsync($"Error processing message: {errormessage} {Environment.NewLine}Details. routingkeyorqueuename: '{routingkeyorqueuename}' | consumertag: {consumertag} | exchange: {exchange} | message: {message}");
        }


        // Application starting point
        public void Run()
        {
            _consumerService.StartConsumers("consumer");

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



        private async Task<Constants.MessageProcessInstruction> ProcessMessage(string routingKeyOrQueueName, string consumerTag, string exchange, string message)
        {

            await Console.Out.WriteLineAsync($"Message received by '{consumerTag}'. Consumer status: ");

            var details = _consumerService.GetConsumerDetails();

            if (details != null)
            {
                foreach (var consumerInfo in details)
                {
                    Console.ForegroundColor = consumerInfo.Id % 2 == 0 ? ConsoleColor.Blue : ConsoleColor.DarkGreen;

                    await Console.Out.WriteLineAsync($"Consumer '{consumerInfo.Name}'; connected to {consumerInfo.ConnectedToHost}:{consumerInfo.ConnectedToPort}; started at {consumerInfo.ConnectionTime:yyyy-MM-dd HH:mm:ss}; {consumerInfo.LastMessageDate:yyyy-MM-dd HH:mm:ss} ");

                    Console.ResetColor();


                }

            }

            switch (message)
            {
                case "ok":
                    _senderService.Send(message);
                    return Constants.MessageProcessInstruction.OK;
                case "ignore":
                    return Constants.MessageProcessInstruction.IgnoreMessage;
                case "requeue":
                    return Constants.MessageProcessInstruction.IgnoreMessageWithRequeue;
                case "delay":
                    return Constants.MessageProcessInstruction.RequeueMessageWithDelay;
                case "error":
                    throw new Exception("error processing message");
                default:
                    return Constants.MessageProcessInstruction.Unknown;
            }

            


        }


        private async Task ProcessShutdown(string consumerTag, ushort errorCode, string shutdownInitiator, string errorMessage)
        {
            await Console.Out.WriteLineAsync($"Shutdown '{consumerTag}' | {errorCode} | {shutdownInitiator} | {errorMessage}");
        }
    }
}