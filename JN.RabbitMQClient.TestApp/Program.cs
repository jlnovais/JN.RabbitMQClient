using System;
using System.IO;
using System.Xml.Schema;
using JN.RabbitMQClient.Extensions;
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
        private static IConfigurationRoot _configuration;
        private static bool _useSenderKeepConnection;
        private static bool overrideKeepConnection;

        static void Main(string[] args)
        {

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            _configuration = builder.Build();



            if (args?.Length>=1)
            {
                _useSenderKeepConnection = args[0] == "keepConnection";
                overrideKeepConnection = true;

                Console.WriteLine($"Starting Test app with parameter keepConnection = {_useSenderKeepConnection} ");
            }

           

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
            services.AddSingleton<IConfiguration>(_configuration);
            
            var configSender = _configuration.GetBrokerConfigSender("BrokerConfigSender");
            if(overrideKeepConnection)
                configSender.KeepConnectionOpen = _useSenderKeepConnection;


            services.AddConsumersService(options =>
            {
                var userOptions = _configuration.GetBrokerConfigConsumers("BrokerConfigConsumers");

                options.RoutingKeyOrQueueName = userOptions.RoutingKeyOrQueueName;
                options.TotalInstances = userOptions.TotalInstances;
                options.Host = userOptions.Host;
                options.Port = userOptions.Port;
                options.Password = userOptions.Password;
                options.Username = userOptions.Username;
                options.ShuffleHostList = userOptions.ShuffleHostList;
                options.UseTLS = userOptions.UseTLS;
                options.VirtualHost = userOptions.VirtualHost;
                options.ConnectionTimeoutSeconds = userOptions.ConnectionTimeoutSeconds;

            });

            services.AddSenderService(configSender);

            var useWindowLimiter = _configuration.GetBool("UseWindowLimiter");

            services.AddSingleton<ILimiter>(_ => useWindowLimiter ? GetLimiter(_configuration) : null);

            return services;
        }
        
        
        private static WindowLimiter GetLimiter(IConfiguration configuration)
        {
            var maxAllowed =  configuration.GetInt("WindowLimiterConfig:MaxAllowed"); // number of items to process in the time window
            var windowSeconds = configuration.GetInt("WindowLimiterConfig:WindowSeconds");

            var deniedInstruction =
                configuration.GetMessageProcessInstruction("WindowLimiterConfig:DeniedProcessInstruction");
                
            return new WindowLimiter(maxAllowed, windowSeconds, deniedInstruction);
        }
    }
}
