using System;
using JN.RabbitMQClient.Entities;
using Microsoft.Extensions.Configuration;

namespace JN.RabbitMQClient.TestApp.HelperClasses
{
    public static class SettingsHelper
    {

        public static BrokerConfig GetBrokerConfig(this IConfiguration configuration, string sectionName)
        {
            var section = configuration.GetSection(sectionName);

            short.TryParse(section["TotalInstances"], out var totalInstances);
            bool.TryParse(section["ShuffleHostList"], out var shuffleHostList);

            var conf = new BrokerConfig()
            {
                Host = section["Host"],
                Port = Convert.ToInt16(section["Port"]),
                Password = section["Password"],
                VirtualHost = section["VirtualHost"],
                Username = section["Username"],
                Exchange = section["Exchange"],
                RoutingKeyOrQueueName = section["RoutingKeyOrQueueName"],
                TotalInstances = totalInstances,
                ShuffleHostList = shuffleHostList

            };

            return conf;
        }
        
    }
}
