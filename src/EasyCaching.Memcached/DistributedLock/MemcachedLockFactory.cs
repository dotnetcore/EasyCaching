using EasyCaching.Core.DistributedLock;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;

namespace EasyCaching.Memcached.DistributedLock
{
    public class MemcachedLockFactory : DistributedLockFactory
    {
        public MemcachedLockFactory(string name,
            IEnumerable<EasyCachingMemcachedClient> memcachedClients,
            IOptionsMonitor<MemcachedOptions> optionsMonitor,
            ILoggerFactory loggerFactory = null)
            : base(new MemcachedLockProvider(name,
                    memcachedClients.Single(x => x.Name.Equals(name))),
                DistributedLockOptions.FromProviderOptions(optionsMonitor.Get(name)),
                loggerFactory)
        { }
    }
}
