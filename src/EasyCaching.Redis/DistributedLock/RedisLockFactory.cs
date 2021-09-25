namespace EasyCaching.Redis.DistributedLock
{
    using EasyCaching.Core.DistributedLock;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using StackExchange.Redis.KeyspaceIsolation;
    using System.Collections.Generic;
    using System.Linq;

    public class RedisLockFactory : DistributedLockFactory
    {
        private readonly IEnumerable<IRedisDatabaseProvider> _dbProviders;

        public RedisLockFactory(string name, IEnumerable<IRedisDatabaseProvider> dbProviders,
            IOptionsMonitor<RedisOptions> optionsMonitor,
            ILoggerFactory loggerFactory = null)
            : base(name, x => DistributedLockOptions.FromProviderOptions(optionsMonitor.Get(x)), loggerFactory) =>
            _dbProviders = dbProviders;

        protected override IDistributedLockProvider GetLockProvider(string name) =>
            new RedisLockProvider(_dbProviders.Single(x => x.DBProviderName.Equals(name)).GetDatabase().WithKeyPrefix(name));
    }
}
