namespace EasyCaching.Memcached
{
    using Enyim.Caching;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using System;

    public static class MemcachedApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseDefaultMemcached(this IApplicationBuilder app)
        {
            try
            {
                app.ApplicationServices.GetService<IMemcachedClient>()
                    .GetAsync<string>("EnyimMemcached").Wait();
                Console.WriteLine("EnyimMemcached Started.");
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
