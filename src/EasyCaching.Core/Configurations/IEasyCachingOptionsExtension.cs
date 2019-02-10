namespace EasyCaching.Core.Configurations
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// EasyCaching options extension.
    /// </summary>
    public interface IEasyCachingOptionsExtension
    {
        /// <summary>
        /// Adds the services.
        /// </summary>
        /// <param name="services">Services.</param>
        void AddServices(IServiceCollection services);

        /// <summary>
        /// Withs the services.
        /// </summary>
        /// <param name="app">App.</param>
        void WithServices(IApplicationBuilder app);
    }
}
