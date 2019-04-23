using System;
using System.Linq;
using RabbitMQ.Client;

namespace JN.RabbitMQClient
{
    public abstract class RabbitMQClientBase : IDisposable
    {
        private ConnectionFactory _connectionFactory;
        private IConnection _connection;
        protected internal IModel _model;


        public string Username { get; set; } = "";
        public string Password { get; set; } = "";

        /// <summary>
        /// if connecting to a cluster, the host can be in the following format [cluster IP1];[cluster IP2];[cluster IP1]; etc... Ex: 192.168.1.1;192.168.1.2;192.168.1.3
        /// </summary>
        public string Host { get; set; } = "";

        public string QueueName { get; set; } = "";

        public string VirtualHost { get; set; } = "";
        public int Port { get; set; } = 0;

        public int ConnectionTimeoutMillisecs { get; set; } = 0;
        public bool ShuffleHostList { get; set; } = false;

        public string ConnectedServer { get; private set; } = "";



        protected virtual void SetupConnection(bool autoclose = false)
        {
            SetupConnection(null, autoclose);
        }

        protected virtual void SetupConnection(string connectionName, bool autoclose = false, bool automaticConnectionRecovery = true)
        {

            ushort Heartbeat = 20;

            if (_model != null)
                return;

            if (_connection != null)
                if (_connection.IsOpen)
                    return;

            if (string.IsNullOrWhiteSpace(Host))
                throw new Exception("Invalid host.");

            var hosts = Utils.GetHostsList(Host);

            if (ShuffleHostList)
                hosts = hosts.OrderBy(x => Guid.NewGuid()).ToList();

            if (hosts.Count == 0)
                throw new Exception("No hosts defined for connection.");

            var totalHosts = hosts.Count;
            var attempt = 0;

            foreach (var host in hosts)
            {
                try
                {
                    _connectionFactory = new ConnectionFactory
                    {
                        HostName = host,
                        UserName = Username,
                        Password = Password,
                        RequestedHeartbeat = Heartbeat,
                        AutomaticRecoveryEnabled = automaticConnectionRecovery,
                    };


                    if (ConnectionTimeoutMillisecs > 0)
                        _connectionFactory.RequestedConnectionTimeout = ConnectionTimeoutMillisecs;

                    if (string.IsNullOrEmpty(VirtualHost) == false)
                        _connectionFactory.VirtualHost = VirtualHost;

                    if (Port > 0)
                        _connectionFactory.Port = Port;

                    _connection = _connectionFactory.CreateConnection(connectionName);


                    _model = _connection.CreateModel();

                    _connection.AutoClose = autoclose;

                    ConnectedServer = _connection.Endpoint.HostName + ":" + _connection.Endpoint.Port;
                    break;
                }
                catch (Exception)
                {
                    if (attempt == totalHosts - 1)
                        throw;

                    attempt++;
                }

            }



        }

        public void Dispose()
        {
            try
            {
                if (_connection != null)
                    if (_connection.IsOpen)
                        _connection.Close();
            }
            catch { }
            

            _connection = null;

            if (_model != null)
                _model.Abort();

            _model = null;
        }
    }
}
