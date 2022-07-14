using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using org.apache.zookeeper;

namespace EasyCaching.Bus.Zookeeper
{
    public class DefaultZookeeperClientFactory : IDisposable, IZookeeperClientFactory
    {
        private ConcurrentDictionary<string, IZookeeperClient> _zookeeperClients = new ConcurrentDictionary<string, IZookeeperClient>();

        private ConcurrentDictionary<string, RegistryCenterHealthCheckModel> m_healthCheck = new ConcurrentDictionary<string, RegistryCenterHealthCheckModel>();

        private ZkBusOptions _zkConfig;
        public ILogger<DefaultZookeeperClientFactory> Logger { get; set; }

        protected string[] ConnectionStrings
        {
            get
            {
                var connectionStrings =
                    _zkConfig?.ConnectionString?.Split(';')?.Where(p => !string.IsNullOrEmpty(p)).ToArray() ??
                    new string[0];
                return connectionStrings;
            }
        }

        public DefaultZookeeperClientFactory(IOptionsMonitor<ZkBusOptions> registryCenterOptions)
        {
            _zkConfig = registryCenterOptions.CurrentValue;

            if (string.IsNullOrEmpty(_zkConfig.ConnectionString))
                throw new ArgumentNullException("zookeeper config connectionString is empty");
            Logger = NullLogger<DefaultZookeeperClientFactory>.Instance;

            CreateZookeeperClients();
        }

        public void CreateZookeeperClients()
        {
            foreach (var connStr in ConnectionStrings)
            {
                if (string.IsNullOrEmpty(connStr)) continue;

                if (!_zookeeperClients.Keys.Contains(connStr))
                {
                    CreateZookeeperClient(connStr);
                }
            }
        }

        private async void CreateZookeeperClient(string connStr)
        {
            var zookeeperClientOptions = new ZkBusOptions(connStr);
            try
            {
                var zookeeperClient = new ZookeeperClient(zookeeperClientOptions);
                zookeeperClient.SubscribeStatusChange(async (client, connectionStateChangeArgs) =>
                {
                    var healthCheckModel = m_healthCheck.GetOrAdd(connStr, new RegistryCenterHealthCheckModel(true, 0));

                    switch (connectionStateChangeArgs.State)
                    {
                        case Watcher.Event.KeeperState.Disconnected:
                        case Watcher.Event.KeeperState.Expired:
                            if (client.WaitForKeeperState(Watcher.Event.KeeperState.SyncConnected,
                                zookeeperClientOptions.ConnectionSpanTimeout))
                            {
                                if (healthCheckModel.HealthType == HealthTypeEnum.Disconnected)
                                {
                                    Logger.LogError("zookeeper server disconnected");
                                }

                                healthCheckModel.SetHealth();
                            }
                            else
                            {
                                healthCheckModel.SetUnHealth(HealthTypeEnum.Disconnected,
                                    "Connection session disconnected");
                                if (healthCheckModel.UnHealthTimes > _zkConfig.HealthyCheckTimes)
                                {
                                    _zookeeperClients.TryRemove(client.Options.ConnectionString, out _);
                                }
                            }

                            break;
                        case Watcher.Event.KeeperState.AuthFailed:
                            healthCheckModel.SetUnHealth(HealthTypeEnum.AuthFailed, "AuthFailed");
                            Logger.LogError("zookeeper server AuthFailed");
                            break;
                        case Watcher.Event.KeeperState.SyncConnected:
                        case Watcher.Event.KeeperState.ConnectedReadOnly:
                            healthCheckModel.SetHealth();
                            break;
                    }
                    await Task.CompletedTask;
                });
                _zookeeperClients.GetOrAdd(connStr, zookeeperClient);
                m_healthCheck.GetOrAdd(connStr, new RegistryCenterHealthCheckModel(true, 0));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex?.Message);
                m_healthCheck.GetOrAdd(connStr, new RegistryCenterHealthCheckModel(false)
                {
                    HealthType = HealthTypeEnum.Disconnected,
                    UnHealthTimes = 1,
                    UnHealthReason = ex.Message
                });
            }
        }

        public IZookeeperClient GetZooKeeperClient()
        {
            if (_zookeeperClients.Count <= 0)
            {
                throw new Exception("There is currently no service registry available");
            }

            if (_zookeeperClients.Count == 1)
            {
                return _zookeeperClients.First().Value;
            }

            return _zookeeperClients.Values.ToArray()[RandomSelectorIndex(0, _zookeeperClients.Count)];
        }

        private int RandomSelectorIndex(int min, int max)
        {
            var random = new Random((int)DateTime.Now.Ticks);
            return random.Next(min, max);
        }

        public IReadOnlyList<IZookeeperClient> GetZooKeeperClients()
        {
            if (_zookeeperClients.Count <= 0)
            {
                throw new Exception("There is currently no service registry available");
            }

            return _zookeeperClients.Values.ToList();
        }

        public RegistryCenterHealthCheckModel GetHealthCheckInfo(IZookeeperClient zookeeperClient)
        {
            m_healthCheck.TryGetValue(zookeeperClient.Options.ConnectionString,out RegistryCenterHealthCheckModel val);
            return val ?? new RegistryCenterHealthCheckModel();
        }

        public void Dispose()
        {
            foreach (var _client in _zookeeperClients)
            {
                _client.Value?.Dispose();
            }
        }
    }
}