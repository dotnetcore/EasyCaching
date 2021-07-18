using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;

namespace EasyCaching.Core.DistributedLock
{
    public abstract class DistributedLockFactory : IDistributedLockFactory
    {
        private readonly Func<string, DistributedLockOptions> _optionsMonitor;
        private readonly ILoggerFactory _loggerFactory;

        protected DistributedLockFactory(Func<string, DistributedLockOptions> optionsMonitor,
            ILoggerFactory loggerFactory = null)
        {
            _optionsMonitor = optionsMonitor;
            _loggerFactory = loggerFactory;
        }

        public IDistributedLock CreateLock(string name, string key) =>
            new DistributedLock(name, key, GetLockProvider(name), _optionsMonitor(name), _loggerFactory);

        protected abstract IDistributedLockProvider GetLockProvider(string name);
    }
}