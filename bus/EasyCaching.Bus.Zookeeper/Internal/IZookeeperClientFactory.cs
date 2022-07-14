using System.Collections.Generic;

namespace EasyCaching.Bus.Zookeeper
{
    public interface IZookeeperClientFactory
    {
        void CreateZookeeperClients();

        IZookeeperClient GetZooKeeperClient();

        IReadOnlyList<IZookeeperClient> GetZooKeeperClients();

        RegistryCenterHealthCheckModel GetHealthCheckInfo(IZookeeperClient zookeeperClient);
    }
}