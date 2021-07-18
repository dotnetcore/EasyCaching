using EasyCaching.Core.DistributedLock;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;

namespace EasyCaching.Memcached.DistributedLock
{
    public class MemcachedLockFactory : DistributedLockFactory
    {
        private readonly IEnumerable<EasyCachingMemcachedClient> _memcachedClients;

        public MemcachedLockFactory(IEnumerable<EasyCachingMemcachedClient> memcachedClients,
            IOptionsMonitor<MemcachedOptions> optionsMonitor,
            ILoggerFactory loggerFactory = null)
            : base(name => DistributedLockOptions.FromProviderOptions(optionsMonitor.Get(name)),
                loggerFactory) =>
            _memcachedClients = memcachedClients;

        protected override IDistributedLockProvider GetLockProvider(string name) =>
            new MemcachedLockProvider(name,
                    _memcachedClients.Single(x => x.Name.Equals(name)));
    }
}
