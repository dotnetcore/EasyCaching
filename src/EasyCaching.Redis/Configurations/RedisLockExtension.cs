namespace Microsoft.Extensions.DependencyInjection
{
    using EasyCaching.Core.Configurations;
    using EasyCaching.Core.DistributedLock;
    using EasyCaching.Redis.DistributedLock;

    internal sealed class RedisLockExtension : IEasyCachingOptionsExtension
    {
        private readonly string _name;

        public RedisLockExtension(string name) => this._name = name;

        /// <summary>
        /// Adds the services.
        /// </summary>
        /// <param name="services">Services.</param>
        public void AddServices(IServiceCollection services) =>
            services.Add(ServiceDescriptor.Singleton<IDistributedLockFactory, RedisLockFactory>(x =>
                ActivatorUtilities.CreateInstance<RedisLockFactory>(x, _name)));
    }
}
