using System.Collections.Generic;
using RabbitMQ.Client;

namespace JN.RabbitMQClient.Other
{
    public class EndpointResolver : IEndpointResolver
    {
        private readonly IEnumerable<AmqpTcpEndpoint> _endPoints;

        public EndpointResolver(IEnumerable<AmqpTcpEndpoint> endPoints)
        {
            _endPoints = endPoints;
        }

        public IEnumerable<AmqpTcpEndpoint> All()
        {
            return _endPoints;
        }
    }
}