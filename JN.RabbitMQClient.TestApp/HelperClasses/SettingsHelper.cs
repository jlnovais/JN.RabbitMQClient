using System;
using JN.RabbitMQClient.Entities;
using Microsoft.Extensions.Configuration;

namespace JN.RabbitMQClient.TestApp.HelperClasses
{
    public static class SettingsHelper
    {

        public static Constants.MessageProcessInstruction GetMessageProcessInstruction(this IConfiguration configuration, string sectionName)
        {
            var instructionStr = configuration[sectionName];

            if (instructionStr != null)
                return (Constants.MessageProcessInstruction)Enum.Parse(typeof(Constants.MessageProcessInstruction), instructionStr);

            return Constants.MessageProcessInstruction.Unknown;
        }

        public static string GetString(this IConfiguration configuration, string sectionName)
        {
            return configuration[sectionName];
        }

        public static byte GetByte(this IConfiguration configuration, string sectionName)
        {
            var res = byte.TryParse(configuration[sectionName], out var value);

            return res ? value : (byte) 0;
        }

        public static int GetInt(this IConfiguration configuration, string sectionName)
        {
            var res = int.TryParse(configuration[sectionName], out var value);

            return res ? value : 0;
        }

        public static bool GetBool(this IConfiguration configuration, string sectionName)
        {
            var res = bool.TryParse(configuration[sectionName], out var value);

            return res && value;
        }


        public static BrokerConfigConsumers GetBrokerConfigConsumers(this IConfiguration configuration,
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
                UseTLS = useTls,
                ConnectionTimeoutSeconds = Convert.ToInt16(section["ConnectionTimeoutSeconds"])
            };

            return conf;
        }

        public static BrokerConfigSender GetBrokerConfigSender(this IConfiguration configuration, string sectionName)
        {
            var section = configuration.GetSection(sectionName);

            bool.TryParse(section["ShuffleHostList"], out var shuffleHostList);
            bool.TryParse(section["UseTLS"], out var useTls);
            bool.TryParse(section["KeepConnectionOpen"], out var keepConnectionOpen);
            bool.TryParse(section["GetQueueInfoOnSend"], out var getQueueInfoOnSend);

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
                KeepConnectionOpen = keepConnectionOpen,
                ConnectionTimeoutSeconds = Convert.ToInt16(section["ConnectionTimeoutSeconds"]),
                GetQueueInfoOnSend = getQueueInfoOnSend

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
