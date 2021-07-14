using EasyCaching.Core.Configurations;
using EasyCaching.Core.DistributedLock;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;

namespace EasyCaching.CSRedis.DistributedLock
{
    public class CSRedisLockFactory : IDistributedLockFactory
    {
        private readonly BaseProviderOptions _options;
        private readonly ILogger<CSRedisLock> _logger;
        private readonly EasyCachingCSRedisClient _cache;

        public CSRedisLockFactory(string name,
            IEnumerable<EasyCachingCSRedisClient> clients,
            IOptionsMonitor<RedisOptions> optionsMonitor,
            ILogger<CSRedisLock> logger = null)
        {
            _cache = clients.Single(x => x.Name.Equals(name));
            _options = optionsMonitor.Get(name);
            _logger = logger;
        }

        public IDistributedLock CreateLock(string name, string key) =>
            new CSRedisLock($"{name}/{key}", _cache, _options, _logger);
    }
}
