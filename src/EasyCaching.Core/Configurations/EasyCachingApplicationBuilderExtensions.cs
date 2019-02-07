namespace EasyCaching.Core
{
    using EasyCaching.Core.Configurations;
    using Microsoft.AspNetCore.Builder;

    public static class EasyCachingApplicationBuilderExtensions
    {
        /// <summary>
        /// Uses the easy caching.
        /// </summary>
        /// <returns>The easy caching.</returns>
        /// <param name="app">App.</param>
        public static IApplicationBuilder UseEasyCaching(this IApplicationBuilder app)
        {
            ArgumentCheck.NotNull(app, nameof(app));

            var options = app.ApplicationServices.GetService(typeof(EasyCachingOptions));

            if (options is EasyCachingOptions cachingOptions)
            {
                foreach (var serviceExtension in cachingOptions.Extensions)
                {
                    serviceExtension.WithServices(app);
                }
            }

            return app;
        }
    }
}
