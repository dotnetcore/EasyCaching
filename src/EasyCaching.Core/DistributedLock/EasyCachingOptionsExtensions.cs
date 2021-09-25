using EasyCaching.Core.Configurations;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace EasyCaching.Core.DistributedLock
{
    public static class EasyCachingOptionsExtensions
    {
        public static EasyCachingOptions UseMemoryLock(this EasyCachingOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            options.RegisterExtension(new MemoryLockExtension());

            return options;
        }
           

        public static EasyCachingOptions UseDistributedLock<T>(this EasyCachingOptions options)
            where T : class, IDistributedLockFactory
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            options.RegisterExtension(new DistributedLockOptionsExtension<T>());

            return options;
        }
    }

    internal sealed class MemoryLockExtension : IEasyCachingOptionsExtension
    {
        public void AddServices(IServiceCollection services) =>
            services.Add(ServiceDescriptor.Singleton<IDistributedLockFactory, MemoryLockFactory>());
    }
}
