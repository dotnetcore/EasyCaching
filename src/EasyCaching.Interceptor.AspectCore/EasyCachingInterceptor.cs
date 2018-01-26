namespace EasyCaching.Interceptor.AspectCore
{
    using EasyCaching.Core;
    using EasyCaching.Core.Internal;
    using global::AspectCore.DynamicProxy;
    using global::AspectCore.Injector;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Easy caching interceptor.
    /// </summary>
    public class EasyCachingInterceptor : AbstractInterceptor
    {
        /// <summary>
        /// Gets or sets the cache provider.
        /// </summary>
        /// <value>The cache provider.</value>
        [FromContainer]
        public IEasyCachingProvider CacheProvider { get; set; }

        /// <summary>
        /// Gets or sets the key generator.
        /// </summary>
        /// <value>The key generator.</value>
        [FromContainer]
        public IEasyCachingKeyGenerator KeyGenerator { get; set; }

        /// <summary>
        /// Invoke the specified context and next.
        /// </summary>
        /// <returns>The invoke.</returns>
        /// <param name="context">Context.</param>
        /// <param name="next">Next.</param>
        public async override Task Invoke(AspectContext context, AspectDelegate next)
        {
            //Process any early evictions 
            await ProcessEvictAsync(context, true);

            //Process any cache interceptor 
            await ProceedAbleAsync(context, next);

            // Process any put requests
            await ProcessPutAsync(context);

            // Process any late evictions
            await ProcessEvictAsync(context, false);
        }

        /// <summary>
        /// Proceeds the able async.
        /// </summary>
        /// <returns>The able async.</returns>
        /// <param name="context">Context.</param>
        /// <param name="next">Next.</param>
        private async Task ProceedAbleAsync(AspectContext context, AspectDelegate next)
        {
            var attribute = context.ServiceMethod.GetCustomAttributes(true).FirstOrDefault(x => x.GetType() == typeof(EasyCachingAbleAttribute)) as EasyCachingAbleAttribute;

            if (attribute != null)
            {
                var cacheKey = KeyGenerator.GetCacheKey(context.ServiceMethod, attribute.CacheKeyPrefix);
                var cacheValue = await CacheProvider.GetAsync<object>(cacheKey);

                if (cacheValue.HasValue)
                {
                    context.ReturnValue = cacheValue.Value;
                }
                else
                {
                    // Invoke the method if we don't have a cache hit
                    await next(context);

                    if (!string.IsNullOrWhiteSpace(cacheKey))
                        await CacheProvider.SetAsync(cacheKey, context.ReturnValue, TimeSpan.FromSeconds(attribute.Expiration));
                }
            }
            else
            {
                // Invoke the method if we don't have EasyCachingAbleAttribute
                await next(context);
            }
        }

        /// <summary>
        /// Processes the put async.
        /// </summary>
        /// <returns>The put async.</returns>
        /// <param name="context">Context.</param>
        private async Task ProcessPutAsync(AspectContext context)
        {
            var attribute = context.ServiceMethod.GetCustomAttributes(true).FirstOrDefault(x => x.GetType() == typeof(EasyCachingPutAttribute)) as EasyCachingPutAttribute;

            if (attribute != null)
            {
                var cacheKey = KeyGenerator.GetCacheKey(context.ServiceMethod, attribute.CacheKeyPrefix);

                await CacheProvider.SetAsync(cacheKey, context.ReturnValue, TimeSpan.FromSeconds(attribute.Expiration));
            }
        }

        /// <summary>
        /// Processes the evict async.
        /// </summary>
        /// <returns>The evict async.</returns>
        /// <param name="context">Context.</param>
        /// <param name="isBefore">If set to <c>true</c> is before.</param>
        private async Task ProcessEvictAsync(AspectContext context, bool isBefore)
        {
            var attribute = context.ServiceMethod.GetCustomAttributes(true).FirstOrDefault(x => x.GetType() == typeof(EasyCachingEvictAttribute)) as EasyCachingEvictAttribute;

            if (attribute != null && attribute.IsBefore == isBefore)
            {
                var cacheKey = KeyGenerator.GetCacheKey(context.ServiceMethod, attribute.CacheKeyPrefix);

                await CacheProvider.RemoveAsync(cacheKey);
            }
        }
    }
}