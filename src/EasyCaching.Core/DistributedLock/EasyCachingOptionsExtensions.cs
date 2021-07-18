using EasyCaching.Core.Configurations;
using System;

namespace EasyCaching.Core.DistributedLock
{
    public static class EasyCachingOptionsExtensions
    {
        public static EasyCachingOptions UseMemoryLock(this EasyCachingOptions options) =>
            options.UseDistributedLock<MemoryLockFactory>();

        public static EasyCachingOptions UseDistributedLock<T>(this EasyCachingOptions options)
            where T : class, IDistributedLockFactory
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            options.RegisterExtension(new DistributedLockOptionsExtension<T>());

            return options;
        }
    }
}
