using EasyCaching.Core.Configurations;
using EasyCaching.Core.DistributedLock;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis.KeyspaceIsolation;
using System.Collections.Generic;
using System.Linq;

namespace EasyCaching.Redis.DistributedLock
{
    public class RedisLockFactory : IDistributedLockFactory
    {
        private readonly BaseProviderOptions _options;
        private readonly ILogger<RedisLock> _logger;
        private readonly IRedisDatabaseProvider _dbProvider;

        public RedisLockFactory(string name,
            IEnumerable<IRedisDatabaseProvider> dbProviders,
            IOptionsMonitor<RedisOptions> optionsMonitor,
            ILogger<RedisLock> logger = null)
        {
            _dbProvider = dbProviders.Single(x => x.DBProviderName.Equals(name));
            _options = optionsMonitor.Get(name);
            _logger = logger;
        }

        public IDistributedLock CreateLock(string name, string key) =>
            new RedisLock(key, _dbProvider.GetDatabase().WithKeyPrefix(name), _options, _logger);
    }
}
