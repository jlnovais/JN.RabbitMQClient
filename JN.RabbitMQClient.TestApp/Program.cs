﻿using System;
using System.IO;
using JN.RabbitMQClient.Interfaces;
using JN.RabbitMQClient.TestApp.HelperClasses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace JN.RabbitMQClient.TestApp
{
    class Program
    {
        private static IConfigurationRoot configuration;

        static void Main(string[] args)
        {
            Console.WriteLine("Test app");


            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            configuration = builder.Build();


            // Create service collection and configure our services
            var services = ConfigureServices();
            // Generate a provider
            var serviceProvider = services.BuildServiceProvider();

            // Kick off our actual code
            serviceProvider.GetService<ConsoleApp>().Run();

        }


        private static IServiceCollection ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();

            services.AddLogging((loggingBuilder) =>
            {
                // configure Logging with NLog
                //loggingBuilder.ClearProviders();
                loggingBuilder.SetMinimumLevel(LogLevel.Trace);
                loggingBuilder.AddConsole();
                loggingBuilder.AddNLog();


            });

            // IMPORTANT! Register our application entry point
            services.AddTransient<ConsoleApp>();

            services.AddSingleton<IRabbitMqConsumerService, RabbitMqConsumerService>();
            services.AddSingleton<IRabbitMqSenderService, RabbitMqSenderService>();
            services.AddSingleton<IBrokerConfigSender>(
                configuration.GetBrokerConfig("BrokerConfigSender"));
            services.AddSingleton<IBrokerConfigConsumers>(
                configuration.GetBrokerConfig("BrokerConfigConsumers"));


            return services;
        }







    }
}
