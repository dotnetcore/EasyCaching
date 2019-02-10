namespace EasyCaching.Core
{
    using System;
    using EasyCaching.Core.Configurations;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// EasyCaching service collection extensions.
    /// </summary>
    public static class EasyCachingServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the easycaching.
        /// </summary>
        /// <returns>The easy caching.</returns>
        /// <param name="services">Services.</param>
        /// <param name="setupAction">Setup action.</param>
        public static IServiceCollection AddEasyCaching(this IServiceCollection services, Action<EasyCachingOptions> setupAction)
        {
            ArgumentCheck.NotNull(setupAction, nameof(setupAction));

            //Options and extension service
            var options = new EasyCachingOptions();
            setupAction(options);
            foreach (var serviceExtension in options.Extensions)
            {
                serviceExtension.AddServices(services);
            }
            services.AddSingleton(options);

            return services;
        }
    }
}
