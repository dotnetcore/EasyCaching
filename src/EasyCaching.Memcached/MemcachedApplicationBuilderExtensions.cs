namespace EasyCaching.Memcached
{
    using Enyim.Caching;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using System;

    /// <summary>
    /// Memcached application builder extensions.
    /// </summary>
    public static class MemcachedApplicationBuilderExtensions
    {
        /// <summary>
        /// Uses the default memcached.
        /// </summary>
        /// <returns>The default memcached.</returns>
        /// <param name="app">App.</param>
        public static IApplicationBuilder UseDefaultMemcached(this IApplicationBuilder app)
        {
            try
            {
                app.ApplicationServices.GetService<IMemcachedClient>()
                    .GetAsync<string>("EnyimMemcached").Wait();
                app.ApplicationServices.GetService<ILogger<IMemcachedClient>>()
                    .LogInformation(new EventId(), "EnyimMemcached Started.");
            }
            catch (Exception ex)
            {
                app.ApplicationServices.GetService<ILogger<IMemcachedClient>>()
                    .LogError(new EventId(), ex, "EnyimMemcached Failed.");
            }
            return app;
        }
    }
}
