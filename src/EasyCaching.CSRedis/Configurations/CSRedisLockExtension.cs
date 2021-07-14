using EasyCaching.Core.Configurations;
using EasyCaching.Core.DistributedLock;
using EasyCaching.CSRedis.DistributedLock;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace
namespace EasyCaching.Redis
{
    /// <summary>
    /// Redis options extension.
    /// </summary>
    internal sealed class CSRedisLockExtension : IEasyCachingOptionsExtension
    {
        /// <summary>
        /// The name.
        /// </summary>
        private readonly string _name;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.Redis.RedisOptionsExtension"/> class.
        /// </summary>
        /// <param name="name">Name.</param>
        public CSRedisLockExtension(string name) => this._name = name;

        /// <summary>
        /// Adds the services.
        /// </summary>
        /// <param name="services">Services.</param>
        public void AddServices(IServiceCollection services) =>
            services.Replace(ServiceDescriptor.Singleton<IDistributedLockFactory, CSRedisLockFactory>(x =>
                ActivatorUtilities.CreateInstance<CSRedisLockFactory>(x, _name)));
    }
}
