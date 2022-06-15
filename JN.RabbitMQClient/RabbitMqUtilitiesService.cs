using System;
using JN.RabbitMQClient.Entities;
using JN.RabbitMQClient.Interfaces;
using JN.RabbitMQClient.Other;
using RabbitMQ.Client;

namespace JN.RabbitMQClient
{
    
    /// <summary>
    /// Utilities service
    /// </summary>
    public class RabbitMqUtilitiesService : RabbitMqServiceBase, IRabbitMqUtilitiesService
    {
        public RabbitMqUtilitiesService(IBrokerConfig config):base(config)
        {
        }


        public Result CreateQueue(string queueName)
        {
            return CreateQueue(queueName, null, null);
        }

        public Result CreateQueue(string queueName, string exchangeToBind)
        {
            return CreateQueue(queueName, exchangeToBind, null);
        }

        public Result CreateQueue(string queueName, string exchangeToBind, string bindRoutingKey)
        {
            var res = new Result();

            try
            {
                using (var connection = GetConnection(ServiceDescription + "_UtilService"))
                using (var channel = connection.CreateModel())
                {
                    RabbitMqUtilities.CreateQueueOrGetInfo(queueName, channel);

                    var key = string.IsNullOrWhiteSpace(bindRoutingKey) ? string.Empty : bindRoutingKey;
                    var exchange = string.IsNullOrWhiteSpace(exchangeToBind) ? string.Empty : exchangeToBind;

                    if (!string.IsNullOrWhiteSpace(exchange))
                        channel.QueueBind(queueName, exchange, key);

                    res.Success = true;
                }
            }
            catch (Exception e)
            {
                res.Success = false;
                res.ErrorDescription = e.Message;
                res.ErrorCode = -1;
            }

            return res;
        }

        public Result DeleteQueue(string queueName)
        {
            var res = new Result();
            try
            {
                using (var connection = GetConnection(ServiceDescription + "_UtilService"))
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDelete(queueName, false, false);
                    res.Success = true;
                }
            }
            catch (Exception e)
            {
                res.Success = false;
                res.ErrorDescription = e.Message;
                res.ErrorCode = -1;
            }

            return res;
        }

        public Result<uint> GetTotalMessages(string queueName)
        {
            var res = new Result<uint>();

            try
            {
                using (var connection = GetConnection(ServiceDescription + "_UtilService"))
                using (var channel = connection.CreateModel())
                {
                    var total = channel.MessageCount(queueName);

                    res.Success = true;
                    res.ReturnedObject = total;
                }
            }
            catch (Exception e)
            {
                res.Success = false;
                res.ErrorCode = -1;
                res.ErrorDescription = e.Message;
            }

            return res;
        }

    }
}
