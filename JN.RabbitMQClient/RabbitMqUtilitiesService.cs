﻿using System;
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

        [Obsolete("GetTotalMessages is deprecated, please use GetQueueInfo instead.")]
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

        public Result<QueueInfo> GetQueueInfo(string queueName)
        {
            try
            {
                using (var connection = GetConnection(ServiceDescription + "_UtilService"))
                using (var channel = connection.CreateModel())
                {
                    return GetQueueInfo(channel, queueName);
                }
            }
            catch (Exception e)
            {
                var res = new Result<QueueInfo>
                {
                    ErrorCode = (int)Constants.Errors.ErrorGettingQueueDetails,
                    Success = false,
                    ErrorDescription = e.Message
                };

                return res;
            }
        }

        public static Result<QueueInfo> GetQueueInfo(IConnection connection, string queueName)
        {
            try
            {
                using (var channel = connection.CreateModel())
                {
                    return GetQueueInfo(channel, queueName);
                }
            }
            catch (Exception e)
            {
                var res = new Result<QueueInfo>
                {
                    ErrorCode = (int)Constants.Errors.ErrorGettingQueueDetails,
                    Success = false,
                    ErrorDescription = e.Message
                };

                return res;
            }

        }

        public static Result<QueueInfo> GetQueueInfo(IModel channel, string queueName)
        {
            var res = new Result<QueueInfo>();

            try
            {
                var statistics = new QueueInfo()
                {
                    ConsumerCount = channel.ConsumerCount(queueName),
                    MessageReadyCount = channel.MessageCount(queueName)
                };


                res.Success = true;
                res.ReturnedObject = statistics;

                return res;
            }
            catch (Exception e)
            {
                res.Success = false;
                res.ErrorCode = (int)Constants.Errors.ErrorGettingQueueDetails;
                res.ErrorDescription = e.Message;
            }

            return res;
        }

    }
}
