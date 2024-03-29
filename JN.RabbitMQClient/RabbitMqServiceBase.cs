﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using JN.RabbitMQClient.Interfaces;
using JN.RabbitMQClient.Other;
using RabbitMQ.Client;

namespace JN.RabbitMQClient
{
    public class RabbitMqServiceBase
    {
        private protected readonly IBrokerConfig _config;

        /// <summary>
        /// Service description - it will be used when a connection is setup - it will be visible on RabbitMq
        /// </summary>
        public string ServiceDescription { get; set; }


        public RabbitMqServiceBase(IBrokerConfig config)
        {
            _config = config;
        }

        internal IConnection GetConnection(string connectionName, bool automaticRecovery = true)
        {
            if (string.IsNullOrWhiteSpace(_config.Host))
                throw new ArgumentException("Invalid host.");

            var factory = new ConnectionFactory
            {
                //HostName = host,
                UserName = _config.Username,
                Password = _config.Password,
                DispatchConsumersAsync = true,
                AutomaticRecoveryEnabled = automaticRecovery
            };

            if (!string.IsNullOrEmpty(_config.VirtualHost))
                factory.VirtualHost = _config.VirtualHost;

            if (_config.ConnectionTimeoutSeconds > 0)
                factory.RequestedConnectionTimeout = TimeSpan.FromSeconds(_config.ConnectionTimeoutSeconds);

            factory.EndpointResolverFactory = GetEndpointResolver;
            var conn = factory.CreateConnection(connectionName);

            return conn;
        }

        private IEndpointResolver GetEndpointResolver(IEnumerable<AmqpTcpEndpoint> arg)
        {
            var hosts = Utils.GetHostsList(_config.Host);

            if (hosts.Count == 0)
                throw new ArgumentException("No hosts defined for connection.");

            var port = AmqpTcpEndpoint.UseDefaultPort;

            if (_config.Port > 0)
                port = _config.Port;

            var endpoints = _config.UseTLS
                ? from host in hosts
                select new AmqpTcpEndpoint
                {
                    HostName = host,
                    Port = port,
                    Ssl = new SslOption
                    {
                        ServerName = host,
                        Enabled = true,
                        AcceptablePolicyErrors = System.Net.Security.SslPolicyErrors.RemoteCertificateChainErrors,
                        //Version = SslProtocols.Tls11
                    }
                }
                : from host in hosts
                select new AmqpTcpEndpoint(host, port);

            if (_config.ShuffleHostList)
                return new DefaultEndpointResolver(endpoints);

            return new EndpointResolver(endpoints);
        }

    }
}