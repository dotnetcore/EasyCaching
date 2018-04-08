namespace EasyCaching.ResponseCaching
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.ResponseCaching;
    using Microsoft.AspNetCore.ResponseCaching.Internal;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using System.Reflection;

    /// <summary>
    /// EasyCaching Response caching middleware.
    /// </summary>
    public class EasyCachingResponseCachingMiddleware : ResponseCachingMiddleware
    {
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:EasyCaching.ResponseCaching.EasyCachingResponseCachingMiddleware"/> class.
        /// </summary>
        /// <param name="next">Next.</param>
        /// <param name="options">Options.</param>
        /// <param name="cache">Cache.</param>
        /// <param name="loggerFactory">Logger factory.</param>
        /// <param name="policyProvider">Policy provider.</param>
        /// <param name="keyProvider">Key provider.</param>
        public EasyCachingResponseCachingMiddleware(
            RequestDelegate next,
            IOptions<ResponseCachingOptions> options,
            IResponseCache cache,
            ILoggerFactory loggerFactory,
            IResponseCachingPolicyProvider policyProvider,
            IResponseCachingKeyProvider keyProvider)
        : base(next, options, loggerFactory, policyProvider, keyProvider)
        {
            FieldInfo cacheFieldInfo = typeof(ResponseCachingMiddleware).GetField("_cache", BindingFlags.NonPublic | BindingFlags.Instance);
            cacheFieldInfo.SetValue(this, cache);
        }
    }
}
