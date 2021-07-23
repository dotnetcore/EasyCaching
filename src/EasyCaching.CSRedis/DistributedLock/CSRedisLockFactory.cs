using EasyCaching.Core.DistributedLock;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;

namespace EasyCaching.CSRedis.DistributedLock
{
    public class CSRedisLockFactory : DistributedLockFactory
    {
        private readonly IEnumerable<EasyCachingCSRedisClient> _clients;

        public CSRedisLockFactory(IEnumerable<EasyCachingCSRedisClient> clients,
            IOptionsMonitor<RedisOptions> optionsMonitor,
            ILoggerFactory loggerFactory = null)
            : base(name => DistributedLockOptions.FromProviderOptions(optionsMonitor.Get(name)), loggerFactory) =>
            _clients = clients;

        protected override IDistributedLockProvider GetLockProvider(string name) =>
            new CSRedisLockProvider(name, _clients.Single(x => x.Name.Equals(name)));
    }
}
