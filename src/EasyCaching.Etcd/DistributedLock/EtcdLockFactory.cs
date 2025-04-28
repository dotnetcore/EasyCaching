using EasyCaching.Core.DistributedLock;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;

namespace EasyCaching.Etcd.DistributedLock
{
    public class EtcdLockFactory : DistributedLockFactory
    {
        private readonly IEnumerable<IEtcdCaching> _etcdClients;

        public EtcdLockFactory(IEnumerable<IEtcdCaching> etcdClients,
            IOptionsMonitor<EtcdCachingOptions> optionsMonitor,
            ILoggerFactory loggerFactory = null)
            : base(name => DistributedLockOptions.FromProviderOptions(optionsMonitor.Get(name)), loggerFactory) =>
            _etcdClients = etcdClients;

        protected override IDistributedLockProvider GetLockProvider(string name) =>
            new EtcdLockProvider(_etcdClients.Single(x => x.ProviderName.Equals(name)));
    }
}
