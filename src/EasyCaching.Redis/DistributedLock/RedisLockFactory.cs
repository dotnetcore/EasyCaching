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
        public RedisLockFactory(string name,
            IEnumerable<IRedisDatabaseProvider> dbProviders,
            IOptionsMonitor<RedisOptions> optionsMonitor,
            ILoggerFactory loggerFactory = null)
        : base(new RedisLockProvider(dbProviders.Single(x => x.DBProviderName.Equals(name)).GetDatabase().WithKeyPrefix(name)),
            DistributedLockOptions.FromProviderOptions(optionsMonitor.Get(name)),
            loggerFactory)
        { }
    }
}
