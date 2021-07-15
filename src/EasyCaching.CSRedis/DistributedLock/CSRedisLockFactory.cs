using EasyCaching.Core.DistributedLock;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;

namespace EasyCaching.CSRedis.DistributedLock
{
    public class CSRedisLockFactory : DistributedLockFactory
    {
        public CSRedisLockFactory(string name,
            IEnumerable<EasyCachingCSRedisClient> clients,
            IOptionsMonitor<RedisOptions> optionsMonitor,
            ILoggerFactory loggerFactory = null)
        : base(new CSRedisLockProvider(name,
                clients.Single(x => x.Name.Equals(name))),
            DistributedLockOptions.FromProviderOptions(optionsMonitor.Get(name)),
            loggerFactory)
        { }
    }
}
