using EasyCaching.Core.Configurations;
using EasyCaching.Core.DistributedLock;
using EasyCaching.Redis.DistributedLock;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

// ReSharper disable once CheckNamespace
namespace EasyCaching.Redis
{
    /// <summary>
    /// Redis options extension.
    /// </summary>
    internal sealed class RedisLockExtension : IEasyCachingOptionsExtension
    {
        /// <summary>
        /// The name.
        /// </summary>
        private readonly string _name;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.Redis.RedisOptionsExtension"/> class.
        /// </summary>
        /// <param name="name">Name.</param>
        public RedisLockExtension(string name) => _name = name;

        /// <summary>
        /// Adds the services.
        /// </summary>
        /// <param name="services">Services.</param>
        public void AddServices(IServiceCollection services) =>
            services.Replace(ServiceDescriptor.Singleton<IDistributedLockFactory, RedisLockFactory>(x =>
                ActivatorUtilities.CreateInstance<RedisLockFactory>(x, _name)));
    }
}
