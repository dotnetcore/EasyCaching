namespace EasyCaching.Core.DistributedLock
{
    using EasyCaching.Core.Configurations;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    internal class DistributedLockOptionsExtension<T> : IEasyCachingOptionsExtension
        where T : class, IDistributedLockFactory
    {
        /// <summary>
        /// Adds the services.
        /// </summary>
        /// <param name="services">Services.</param>
        public void AddServices(IServiceCollection services) =>
            services.Replace(ServiceDescriptor.Singleton<IDistributedLockFactory, T>());
    }
}
