using System;
using JN.RabbitMQClient.Entities;
using JN.RabbitMQClient.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace JN.RabbitMQClient.Extensions
{
    public static class ServicesExtensions
    {
        /// <summary>
        /// Add a RabbitMqConsumerService to IServiceCollection
        /// </summary>
        /// <param name="services"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IServiceCollection AddConsumersService(this IServiceCollection services,
            Action<BrokerConfigConsumers> options = null)
        {
            var myOptions = new BrokerConfigConsumers();
            options?.Invoke(myOptions);

            //configure IOptions<BrokerConfigConsumers> with the DI; can be injected in the controllers constructor
            if (options != null)
                services.Configure(options);

            services.AddSingleton<IRabbitMqConsumerService>(new RabbitMqConsumerService(myOptions));
  
            return services;
        }

        /// <summary>
        /// Add a RabbitMqConsumerService to IServiceCollection
        /// </summary>
        /// <param name="services"></param>
        /// <param name="userOptions"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static IServiceCollection AddConsumersService(this IServiceCollection services,
            BrokerConfigConsumers userOptions)
        {

            if (userOptions == null)
            {
                throw new ArgumentException("Options is null");
            }

            //configure IOptions<BrokerConfigConsumers> with the DI; can be injected in the controllers constructor
            services.Configure<BrokerConfigConsumers>(options =>
            {
                options.RoutingKeyOrQueueName = userOptions.RoutingKeyOrQueueName;
                options.TotalInstances = userOptions.TotalInstances;
                options.Host = userOptions.Host;
                options.Port = userOptions.Port;
                options.Password = userOptions.Password;
                options.Username = userOptions.Username;
                options.ShuffleHostList = userOptions.ShuffleHostList;
                options.UseTLS = userOptions.UseTLS;
                options.VirtualHost = userOptions.VirtualHost;
            });

            services.AddSingleton<IRabbitMqConsumerService>(new RabbitMqConsumerService(userOptions));

            return services;
        }

        /// <summary>
        /// Add a RabbitMqSenderService to IServiceCollection 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IServiceCollection AddSenderService(this IServiceCollection services,
            Action<BrokerConfigSender> options = null)
        {
            var myOptions = new BrokerConfigSender();
            options?.Invoke(myOptions);

            //configure IOptions<BrokerConfigSender> with the DI; can be injected in the controllers constructor
            if (options != null)
                services.Configure(options);

            services.AddSingleton<IRabbitMqSenderService>(new RabbitMqSenderService(myOptions));

            return services;
        }

        /// <summary>
        /// Add a RabbitMqSenderService to IServiceCollection 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="userOptions"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static IServiceCollection AddSenderService(this IServiceCollection services,
            BrokerConfigSender userOptions)
        {

            if (userOptions == null)
            {
                throw new ArgumentException("Options is null");
            }

            //configure IOptions<BrokerConfigSender> with the DI; can be injected in the controllers constructor
            services.Configure<BrokerConfigSender>(options =>
            {
                options.RoutingKeyOrQueueName = userOptions.RoutingKeyOrQueueName;
                options.Host = userOptions.Host;
                options.Port = userOptions.Port;
                options.Password = userOptions.Password;
                options.Username = userOptions.Username;
                options.ShuffleHostList = userOptions.ShuffleHostList;
                options.UseTLS = userOptions.UseTLS;
                options.VirtualHost = userOptions.VirtualHost;
                options.Exchange = userOptions.Exchange;
                options.KeepConnectionOpen = userOptions.KeepConnectionOpen;
            });

            services.AddSingleton<IRabbitMqSenderService>(new RabbitMqSenderService(userOptions));

            return services;
        }


    }
}
