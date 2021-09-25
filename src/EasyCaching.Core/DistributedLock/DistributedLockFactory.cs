using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;

namespace EasyCaching.Core.DistributedLock
{
    public abstract class DistributedLockFactory : IDistributedLockFactory
    {
        private readonly Func<string, DistributedLockOptions> _optionsMonitor;
        private readonly ILoggerFactory _loggerFactory;
        private readonly string _name;

        protected DistributedLockFactory(string name, Func<string, DistributedLockOptions> optionsMonitor,
            ILoggerFactory loggerFactory = null)
        {
            _name = name;
            _optionsMonitor = optionsMonitor;
            _loggerFactory = loggerFactory;
        }

        public string Name => _name;

        public IDistributedLock CreateLock(string name, string key) =>
            new DistributedLock(name, key, GetLockProvider(name), _optionsMonitor(name), _loggerFactory);

        public IDistributedLock CreateLock(string key) =>
            new DistributedLock(_name, key, GetLockProvider(_name), _optionsMonitor(_name), _loggerFactory);

        protected abstract IDistributedLockProvider GetLockProvider(string name);
    }
}