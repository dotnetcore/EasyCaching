namespace EasyCaching.Interceptor.AspectCore
{
    using EasyCaching.Core;
    using EasyCaching.Core.Configurations;
    using EasyCaching.Core.Interceptor;
    using global::AspectCore.DependencyInjection;
    using global::AspectCore.DynamicProxy;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    /// <summary>
    /// Easy caching interceptor.
    /// </summary>
    public class EasyCachingInterceptor : AbstractInterceptor
    {
        /// <summary>
        /// Gets or sets the cache provider factory.
        /// </summary>
        /// <value>The cache provider.</value>
        [FromServiceContext]
        public IEasyCachingProviderFactory CacheProviderFactory { get; set; }

        /// <summary>
        /// Gets or sets the hybrid caching provider.
        /// </summary>
        /// <value>The hybrid caching provider.</value>
        [FromServiceContext]
        public IHybridCachingProvider HybridCachingProvider { get; set; }

        /// <summary>
        /// Gets or sets the key generator.
        /// </summary>
        /// <value>The key generator.</value>
        [FromServiceContext]
        public IEasyCachingKeyGenerator KeyGenerator { get; set; }

        /// <summary>
        /// Get or set the options
        /// </summary>
        [FromServiceContext]
        public IOptions<EasyCachingInterceptorOptions> Options { get; set; }

        /// <summary>
        /// logger
        /// </summary>
        [FromServiceContext]
        public ILogger<EasyCachingInterceptor> Logger { get; set; }

        /// <summary>
        /// The typeof task result method.
        /// </summary>
        private static readonly ConcurrentDictionary<Type, MethodInfo>
                    TypeofTaskResultMethod = new ConcurrentDictionary<Type, MethodInfo>();

        /// <summary>
        /// The typeof task result method.
        /// </summary>
        private static readonly ConcurrentDictionary<MethodInfo, object[]>
                    MethodAttributes = new ConcurrentDictionary<MethodInfo, object[]>();

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

        private object[] GetMethodAttributes(MethodInfo mi)
        {
            return MethodAttributes.GetOrAdd(mi, mi.GetCustomAttributes(true));
        }

        /// <summary>
        /// Proceeds the able async.
        /// </summary>
        /// <returns>The able async.</returns>
        /// <param name="context">Context.</param>
        /// <param name="next">Next.</param>
        private async Task ProceedAbleAsync(AspectContext context, AspectDelegate next)
        {
            if (GetMethodAttributes(context.ServiceMethod).FirstOrDefault(x => typeof(EasyCachingAbleAttribute).IsAssignableFrom(x.GetType())) is EasyCachingAbleAttribute attribute)
            {
                var returnType = context.IsAsync()
                        ? context.ServiceMethod.ReturnType.GetGenericArguments().First()
                        : context.ServiceMethod.ReturnType;

                var cacheKey = KeyGenerator.GetCacheKey(context.ServiceMethod, context.Parameters, attribute.CacheKeyPrefix);

                object cacheValue = null;
                var isAvailable = true;
                try
                {
                    if (attribute.IsHybridProvider)
                    {
                        cacheValue = await HybridCachingProvider.GetAsync(cacheKey, returnType);
                    }
                    else
                    {
                        var _cacheProvider = CacheProviderFactory.GetCachingProvider(attribute.CacheProviderName ?? Options.Value.CacheProviderName);
                        cacheValue = await _cacheProvider.GetAsync(cacheKey, returnType);
                    }
                }
                catch (Exception ex)
                {
                    if (!attribute.IsHighAvailability)
                    {
                        throw;
                    }
                    else
                    {
                        isAvailable = false;
                        Logger?.LogError(new EventId(), ex, $"Cache provider get error.");
                    }
                }

                if (cacheValue != null)
                {
                    if (context.IsAsync())
                    {
                        //#1
                        //dynamic member = context.ServiceMethod.ReturnType.GetMember("Result")[0];
                        //dynamic temp = System.Convert.ChangeType(cacheValue.Value, member.PropertyType);
                        //context.ReturnValue = System.Convert.ChangeType(Task.FromResult(temp), context.ServiceMethod.ReturnType);

                        //#2                                               
                        context.ReturnValue =
                            TypeofTaskResultMethod.GetOrAdd(returnType, t => typeof(Task).GetMethods().First(p => p.Name == "FromResult" && p.ContainsGenericParameters).MakeGenericMethod(returnType)).Invoke(null, new object[] { cacheValue });
                    }
                    else
                    {
                        //context.ReturnValue = System.Convert.ChangeType(cacheValue.Value, context.ServiceMethod.ReturnType);
                        context.ReturnValue = cacheValue;
                    }
                }
                else
                {
                    // Invoke the method if we don't have a cache hit
                    await next(context);

                    if (isAvailable)
                    {
                        // get the result
                        var returnValue = context.IsAsync()
                            ? await context.UnwrapAsyncReturnValue()
                            : context.ReturnValue;

                        // should we do something when method return null?
                        // 1. cached a null value for a short time
                        // 2. do nothing
                        if (returnValue != null)
                        {
                            if (attribute.IsHybridProvider)
                            {
                                await HybridCachingProvider.SetAsync(cacheKey, returnValue, TimeSpan.FromSeconds(attribute.Expiration));
                            }
                            else
                            {
                                var _cacheProvider = CacheProviderFactory.GetCachingProvider(attribute.CacheProviderName ?? Options.Value.CacheProviderName);
                                await _cacheProvider.SetAsync(cacheKey, returnValue, TimeSpan.FromSeconds(attribute.Expiration));
                            }
                        }
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
            if (GetMethodAttributes(context.ServiceMethod).FirstOrDefault(x => typeof(EasyCachingPutAttribute).IsAssignableFrom(x.GetType())) is EasyCachingPutAttribute attribute && context.ReturnValue != null)
            {
                var cacheKey = KeyGenerator.GetCacheKey(context.ServiceMethod, context.Parameters, attribute.CacheKeyPrefix);

                try
                {
                    // get the result
                    var returnValue = context.IsAsync()
                        ? await context.UnwrapAsyncReturnValue()
                        : context.ReturnValue;

                    if (attribute.IsHybridProvider)
                    {
                        await HybridCachingProvider.SetAsync(cacheKey, returnValue, TimeSpan.FromSeconds(attribute.Expiration));
                    }
                    else
                    {
                        var _cacheProvider = CacheProviderFactory.GetCachingProvider(attribute.CacheProviderName ?? Options.Value.CacheProviderName);
                        await _cacheProvider.SetAsync(cacheKey, returnValue, TimeSpan.FromSeconds(attribute.Expiration));
                    }
                }
                catch (Exception ex)
                {
                    if (!attribute.IsHighAvailability) throw;
                    else Logger?.LogError(new EventId(), ex, $"Cache provider set error.");
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
            if (GetMethodAttributes(context.ServiceMethod).FirstOrDefault(x => typeof(EasyCachingEvictAttribute).IsAssignableFrom(x.GetType())) is EasyCachingEvictAttribute attribute && attribute.IsBefore == isBefore)
            {
                try
                {
                    if (attribute.IsAll)
                    {
                        // If is all , clear all cached items which cachekey start with the prefix.
                        var cachePrefix = KeyGenerator.GetCacheKeyPrefix(context.ServiceMethod, attribute.CacheKeyPrefix);

                        if (attribute.IsHybridProvider)
                        {
                            await HybridCachingProvider.RemoveByPrefixAsync(cachePrefix);
                        }
                        else
                        {
                            var _cacheProvider = CacheProviderFactory.GetCachingProvider(attribute.CacheProviderName ?? Options.Value.CacheProviderName);
                            await _cacheProvider.RemoveByPrefixAsync(cachePrefix);
                        }
                    }
                    else
                    {
                        // If not all , just remove the cached item by its cachekey.
                        var cacheKey = KeyGenerator.GetCacheKey(context.ServiceMethod, context.Parameters, attribute.CacheKeyPrefix);

                        if (attribute.IsHybridProvider)
                        {
                            await HybridCachingProvider.RemoveAsync(cacheKey);
                        }
                        else
                        {
                            var _cacheProvider = CacheProviderFactory.GetCachingProvider(attribute.CacheProviderName ?? Options.Value.CacheProviderName);
                            await _cacheProvider.RemoveAsync(cacheKey);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (!attribute.IsHighAvailability) throw;
                    else Logger?.LogError(new EventId(), ex, $"Cache provider remove error.");
                }
            }
        }
    }
}
