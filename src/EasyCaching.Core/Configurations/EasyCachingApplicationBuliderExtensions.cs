namespace EasyCaching.Core
{
    using Microsoft.AspNetCore.Builder;
    using System;

    public static class EasyCachingApplicationBuliderExtensions
    {
        /// <summary>
        /// Uses the easy caching.
        /// </summary>
        /// <returns>The easy caching.</returns>
        /// <param name="app">App.</param>
        public static IApplicationBuilder UseEasyCaching(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            var options = app.ApplicationServices.GetService(typeof(EasyCachingOptions));

            if (options != null && options is EasyCachingOptions)
            {
                foreach (var serviceExtension in ((EasyCachingOptions)options).Extensions)
                {
                    serviceExtension.WithServices(app);
                }
            }

            return app;
        }
    }
}
