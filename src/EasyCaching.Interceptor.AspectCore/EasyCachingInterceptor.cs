namespace EasyCaching.Interceptor.AspectCore
{
    using EasyCaching.Core;
    using EasyCaching.Core.Internal;
    using global::AspectCore.DynamicProxy;
    using global::AspectCore.Extensions.Reflection;
    using global::AspectCore.Injector;
    using System;   
    using System.Linq;
    using System.Reflection;
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
            if (context.ServiceMethod.GetCustomAttributes(true).FirstOrDefault(x => x.GetType() == typeof(EasyCachingAbleAttribute)) is EasyCachingAbleAttribute attribute)
            {
                var cacheKey = KeyGenerator.GetCacheKey(context.ServiceMethod, context.Parameters, attribute.CacheKeyPrefix);
                var cacheValue = await CacheProvider.GetAsync<object>(cacheKey);

                if (cacheValue.HasValue)
                {
                    if (context.IsAsync())
                    {
                        //#1
                        dynamic member = context.ServiceMethod.ReturnType.GetMember("Result")[0];
                        dynamic temp = System.Convert.ChangeType(cacheValue.Value, member.PropertyType);
                        context.ReturnValue = System.Convert.ChangeType(Task.FromResult(temp), context.ServiceMethod.ReturnType);

                        //#2                       
                        //...
                    }
                    else
                    {
                        context.ReturnValue = System.Convert.ChangeType(cacheValue.Value, context.ServiceMethod.ReturnType);
                    }
                }
                else
                {
                    // Invoke the method if we don't have a cache hit
                    await next(context);

                    if (context.IsAsync())
                    {
                        //get the result
                        var returnValue = await context.UnwrapAsyncReturnValue();

                        await CacheProvider.SetAsync(cacheKey, returnValue, TimeSpan.FromSeconds(attribute.Expiration));
                    }
                    else
                    {
                        await CacheProvider.SetAsync(cacheKey, context.ReturnValue, TimeSpan.FromSeconds(attribute.Expiration));
                    }
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
            if (context.ServiceMethod.GetCustomAttributes(true).FirstOrDefault(x => x.GetType() == typeof(EasyCachingPutAttribute)) is EasyCachingPutAttribute attribute && context.ReturnValue != null)
            {
                var cacheKey = KeyGenerator.GetCacheKey(context.ServiceMethod, context.Parameters, attribute.CacheKeyPrefix);

                if (context.IsAsync())
                {
                    //get the result
                    var returnValue = await context.UnwrapAsyncReturnValue();

                    await CacheProvider.SetAsync(cacheKey, returnValue, TimeSpan.FromSeconds(attribute.Expiration));
                }
                else
                {
                    await CacheProvider.SetAsync(cacheKey, context.ReturnValue, TimeSpan.FromSeconds(attribute.Expiration));
                }
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
            if (context.ServiceMethod.GetCustomAttributes(true).FirstOrDefault(x => x.GetType() == typeof(EasyCachingEvictAttribute)) is EasyCachingEvictAttribute attribute && attribute.IsBefore == isBefore)
            {
                if (attribute.IsAll)
                {
                    //If is all , clear all cached items which cachekey start with the prefix.
                    var cachePrefix = KeyGenerator.GetCacheKeyPrefix(context.ServiceMethod, attribute.CacheKeyPrefix);

                    await CacheProvider.RemoveByPrefixAsync(cachePrefix);
                }
                else
                {
                    //If not all , just remove the cached item by its cachekey.
                    var cacheKey = KeyGenerator.GetCacheKey(context.ServiceMethod, context.Parameters, attribute.CacheKeyPrefix);

                    await CacheProvider.RemoveAsync(cacheKey);
                }
            }
        }
    }
}
