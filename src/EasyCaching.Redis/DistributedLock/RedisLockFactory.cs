using EasyCaching.Core.DistributedLock;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis.KeyspaceIsolation;
using System.Collections.Generic;
using System.Linq;

namespace EasyCaching.Redis.DistributedLock
{
    public class RedisLockFactory : DistributedLockFactory
    {
        private readonly IEnumerable<IRedisDatabaseProvider> _dbProviders;

        public RedisLockFactory(IEnumerable<IRedisDatabaseProvider> dbProviders,
            IOptionsMonitor<RedisOptions> optionsMonitor,
            ILoggerFactory loggerFactory = null)
            : base(name => DistributedLockOptions.FromProviderOptions(optionsMonitor.Get(name)), loggerFactory) =>
            _dbProviders = dbProviders;

        protected override IDistributedLockProvider GetLockProvider(string name) =>
            new RedisLockProvider(_dbProviders.Single(x => x.DBProviderName.Equals(name)).GetDatabase().WithKeyPrefix(name));
    }
}
