namespace Microsoft.Extensions.DependencyInjection
{
    using EasyCaching.Core.Configurations;
    using EasyCaching.Core.DistributedLock;
    using EasyCaching.CSRedis.DistributedLock;

    internal sealed class CSRedisLockExtension : IEasyCachingOptionsExtension
    {
        private readonly string _name;

        public CSRedisLockExtension(string name) => this._name = name;

        /// <summary>
        /// Adds the services.
        /// </summary>
        /// <param name="services">Services.</param>
        public void AddServices(IServiceCollection services) =>
            services.Add(ServiceDescriptor.Singleton<IDistributedLockFactory, CSRedisLockFactory>(x =>
                ActivatorUtilities.CreateInstance<CSRedisLockFactory>(x, _name)));
    }
}