namespace EasyCaching.ResponseCaching
{
    using EasyCaching.Core;
    using Microsoft.AspNetCore.Builder;

    /// <summary>
    /// EasyCaching response caching extensions.
    /// </summary>
    public static class EasyCachingResponseCachingExtensions
    {
        /// <summary>
        /// Uses the easy caching response caching.
        /// </summary>
        /// <returns>The easy caching response caching.</returns>
        /// <param name="app">App.</param>
        public static IApplicationBuilder UseEasyCachingResponseCaching(this IApplicationBuilder app)
        {
            ArgumentCheck.NotNull(app, nameof(app));

            return app.UseMiddleware<EasyCachingResponseCachingMiddleware>();
        }
    }
}
