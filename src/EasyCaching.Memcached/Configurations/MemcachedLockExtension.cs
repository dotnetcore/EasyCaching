using EasyCaching.Core.Configurations;
using EasyCaching.Core.DistributedLock;
using EasyCaching.Memcached.DistributedLock;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace
namespace EasyCaching.Memcached
{
    /// <summary>
    /// Redis options extension.
    /// </summary>
    internal sealed class MemcachedLockExtension : IEasyCachingOptionsExtension
    {
        /// <summary>
        /// The name.
        /// </summary>
        private readonly string _name;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.Redis.RedisOptionsExtension"/> class.
        /// </summary>
        /// <param name="name">Name.</param>
        public MemcachedLockExtension(string name) => _name = name;

        /// <summary>
        /// Adds the services.
        /// </summary>
        /// <param name="services">Services.</param>
        public void AddServices(IServiceCollection services) =>
            services.Replace(ServiceDescriptor.Singleton<IDistributedLockFactory, MemcachedLockFactory>(x =>
                ActivatorUtilities.CreateInstance<MemcachedLockFactory>(x, _name)));
    }
}
