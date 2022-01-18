using System;
using System.IO;
using JN.RabbitMQClient.Interfaces;
using JN.RabbitMQClient.Limiter;
using JN.RabbitMQClient.TestApp.HelperClasses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace JN.RabbitMQClient.TestApp
{
    public static class Program
    {
        private static IConfigurationRoot configuration;
        private static bool _useSenderKeepConnection = false;

        static void Main(string[] args)
        {

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            configuration = builder.Build();



            if (args?.Length>=1)
            {
                _useSenderKeepConnection = args[0] == "keepConnection";
            }

            Console.WriteLine($"Starting Test app with parameter keepConnection = {_useSenderKeepConnection} ");

            // Create service collection and configure our services
            var services = ConfigureServices();
            // Generate a provider
            var serviceProvider = services.BuildServiceProvider();

            // Kick off our actual code

            var app = serviceProvider.GetService<ConsoleApp>();

            app!.Run();

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();

            app.Stop();

            Console.WriteLine("Finished!!");
        }


        private static IServiceCollection ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();

            services.AddLogging((loggingBuilder) =>
            {
                loggingBuilder.SetMinimumLevel(LogLevel.Trace);
                loggingBuilder.AddConsole();
                loggingBuilder.AddNLog();


            });

            // IMPORTANT! Register our application entry point
            services.AddTransient<ConsoleApp>();

            var configSender = configuration.GetBrokerConfigSender("BrokerConfigSender");
            configSender.KeepConnectionOpen = _useSenderKeepConnection;

            services.AddSingleton<IRabbitMqConsumerService, RabbitMqConsumerService>();
            services.AddSingleton<IRabbitMqSenderService, RabbitMqSenderService>();
            services.AddSingleton<IBrokerConfigSender>(configSender);
            services.AddSingleton<IBrokerConfigConsumers>(
                configuration.GetBrokerConfigConfigConsumers("BrokerConfigConsumers"));

            services.AddSingleton<AppConfig>(configuration.GetAppConfig("BrokerConfigConsumersRetry"));

            services.AddSingleton<ILimiter>(GetLimiter());


            return services;
        }

        private static WindowLimiter GetLimiter()
        {
            const int maxAllowed = 1; // number of items to process in the time window
            const int windowSeconds = 1;
            const Constants.MessageProcessInstruction deniedInstruction = Constants.MessageProcessInstruction.RequeueMessageWithDelay;

            return new WindowLimiter(maxAllowed, windowSeconds, deniedInstruction);
        }
    }
}
