namespace Microsoft.Extensions.DependencyInjection
{
    using EasyCaching.Core.Configurations;
    using EasyCaching.Core.DistributedLock;
    using EasyCaching.Memcached.DistributedLock;

    internal sealed class MemcachedLockExtension : IEasyCachingOptionsExtension
    {
        private readonly string _name;

        public MemcachedLockExtension(string name) => this._name = name;

        /// <summary>
        /// Adds the services.
        /// </summary>
        /// <param name="services">Services.</param>
        public void AddServices(IServiceCollection services) =>
            services.Add(ServiceDescriptor.Singleton<IDistributedLockFactory, MemcachedLockFactory>(x =>
                ActivatorUtilities.CreateInstance<MemcachedLockFactory>(x, _name)));
    }
}
