using Microsoft.Extensions.Logging;

namespace EasyCaching.Core.DistributedLock
{
    public abstract class DistributedLockFactory : IDistributedLockFactory
    {
        private readonly IDistributedLockProvider _provider;
        private readonly DistributedLockOptions _options;
        private readonly ILoggerFactory _loggerFactory;

        protected DistributedLockFactory(IDistributedLockProvider provider,
            DistributedLockOptions options,
            ILoggerFactory loggerFactory = null)
        {
            _provider = provider;
            _options = options;
            _loggerFactory = loggerFactory;
        }

        public IDistributedLock CreateLock(string name, string key) =>
            new DistributedLock(key, _provider, _options, _loggerFactory);
    }
}