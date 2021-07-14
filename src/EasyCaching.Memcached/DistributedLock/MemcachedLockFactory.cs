using EasyCaching.Core.Configurations;
using EasyCaching.Core.DistributedLock;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;

namespace EasyCaching.Memcached.DistributedLock
{
    public class MemcachedLockFactory : IDistributedLockFactory
    {
        private readonly BaseProviderOptions _options;
        private readonly ILogger<MemcachedLock> _logger;
        private readonly EasyCachingMemcachedClient _memcachedClient;

        public MemcachedLockFactory(string name,
            IEnumerable<EasyCachingMemcachedClient> memcachedClients,
            IOptionsMonitor<MemcachedOptions> optionsMonitor,
            ILogger<MemcachedLock> logger = null)
        {
            _memcachedClient = memcachedClients.Single(x => x.Name.Equals(name));
            _options = optionsMonitor.Get(name);
            _logger = logger;
        }

        public IDistributedLock CreateLock(string name, string key) =>
            new MemcachedLock($"{name}/{key}", _memcachedClient, _options, _logger);
    }
}
