using EasyCaching.Core.Configurations;
using System;

namespace EasyCaching.Core.DistributedLock
{
    public static class EasyCachingOptionsExtensions
    {
        public static EasyCachingOptions UseMemoryLock(this EasyCachingOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            options.RegisterExtension(new DistributedLockOptionsExtension<MemoryLockFactory>());

            return options;
        }
    }
}
