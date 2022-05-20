using System;
using JN.RabbitMQClient.Entities;
using Microsoft.Extensions.Configuration;

namespace JN.RabbitMQClient.TestApp.HelperClasses
{
    public static class SettingsHelper
    {
        public static string GetString(this IConfiguration configuration, string sectionName)
        {
            return configuration[sectionName];
        }

        public static BrokerConfigConsumers GetBrokerConfigConfigConsumers(this IConfiguration configuration,
            string sectionName)
        {
            var section = configuration.GetSection(sectionName);

            byte.TryParse(section["TotalInstances"], out var totalInstances);
            bool.TryParse(section["ShuffleHostList"], out var shuffleHostList);

            bool.TryParse(section["UseTLS"], out var useTls);
            
            var conf = new BrokerConfigConsumers
            {
                Host = section["Host"],
                Port = Convert.ToInt16(section["Port"]),
                Password = section["Password"],
                VirtualHost = section["VirtualHost"],
                Username = section["Username"],
                RoutingKeyOrQueueName = section["RoutingKeyOrQueueName"],
                TotalInstances = totalInstances,
                ShuffleHostList = shuffleHostList,
                UseTLS = useTls
            };

            return conf;
        }

        public static BrokerConfigSender GetBrokerConfigSender(this IConfiguration configuration, string sectionName)
        {
            var section = configuration.GetSection(sectionName);

            bool.TryParse(section["ShuffleHostList"], out var shuffleHostList);
            bool.TryParse(section["UseTLS"], out var useTls);
            bool.TryParse(section["KeepConnectionOpen"], out var keepConnectionOpen);

            var conf = new BrokerConfigSender
            {
                Host = section["Host"],
                Port = Convert.ToInt16(section["Port"]),
                Password = section["Password"],
                VirtualHost = section["VirtualHost"],
                Username = section["Username"],
                Exchange = section["Exchange"],
                RoutingKeyOrQueueName = section["RoutingKeyOrQueueName"],
                ShuffleHostList = shuffleHostList,
                UseTLS = useTls,
                KeepConnectionOpen = keepConnectionOpen

            };

            return conf;
        }

        public static BrokerConfigConsumersRetry GetBrokerConfigConsumersRetry(this IConfiguration configuration, string sectionName)
        {
            var section = configuration.GetSection(sectionName);

            var config = new BrokerConfigConsumersRetry
            {
                BrokerRetentionPeriodInRetryQueueSeconds = Convert.ToInt16(section["RetentionPeriodInRetryQueueSeconds"]),
                BrokerRetentionPeriodInRetryQueueSecondsMax = Convert.ToInt16(section["RetentionPeriodInRetryQueueSecondsMax"]),
                BrokerMessageTTLSeconds = Convert.ToInt16(section["MessageTTLSeconds"]),
                BrokerRetryQueue = section["RetryQueue"]
            };

            return config;

        }


    }
}
