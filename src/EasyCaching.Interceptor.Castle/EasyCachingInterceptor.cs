namespace EasyCaching.Interceptor.Castle
{
    using EasyCaching.Core;
    using EasyCaching.Core.Configurations;
    using EasyCaching.Core.Interceptor;
    using global::Castle.DynamicProxy;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    /// <summary>
    /// Easycaching interceptor.
    /// </summary>
    public class EasyCachingInterceptor : IInterceptor
    {
        /// <summary>
        /// The key generator.
        /// </summary>
        private readonly IEasyCachingKeyGenerator _keyGenerator;

        /// <summary>
        /// The cache provider.
        /// </summary>
        private readonly IEasyCachingProvider _cacheProvider;

        /// <summary>
        /// logger
        /// </summary>
        public ILogger<EasyCachingInterceptor> _logger;
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
        /// Initializes a new instance of the <see cref="T:EasyCaching.Interceptor.Castle.EasyCachingInterceptor"/> class.
        /// </summary>
        /// <param name="cacheProviderFactory">Cache provider factory.</param>
        /// <param name="keyGenerator">Key generator.</param>
        /// <param name="options">Options.</param>
        /// <param name="logger">logger </param>
        public EasyCachingInterceptor(IEasyCachingProviderFactory cacheProviderFactory, IEasyCachingKeyGenerator keyGenerator, IOptions<EasyCachingInterceptorOptions> options, ILogger<EasyCachingInterceptor> logger = null)
        {
            _cacheProvider = cacheProviderFactory.GetCachingProvider(options.Value.CacheProviderName);
            _keyGenerator = keyGenerator;
            _logger = logger;
        }

        /// <summary>
        /// Intercept the specified invocation.
        /// </summary>
        /// <returns>The intercept.</returns>
        /// <param name="invocation">Invocation.</param>
        public void Intercept(IInvocation invocation)
        {
            //Process any early evictions 
            ProcessEvict(invocation, true);

            //Process any cache interceptor 
            ProceedAble(invocation);

            // Process any put requests
            ProcessPut(invocation);

            // Process any late evictions
            ProcessEvict(invocation, false);
        }
        private object[] GetMethodAttributes(MethodInfo mi)
        {
            return MethodAttributes.GetOrAdd(mi, mi.GetCustomAttributes(true));
        }

        /// <summary>
        /// Proceeds the able.
        /// </summary>
        /// <param name="invocation">Invocation.</param>
        private void ProceedAble(IInvocation invocation)
        {
            var serviceMethod = invocation.Method ?? invocation.MethodInvocationTarget;

            if (GetMethodAttributes(serviceMethod).FirstOrDefault(x => x.GetType() == typeof(EasyCachingAbleAttribute)) is EasyCachingAbleAttribute attribute)
            {
                var returnType = serviceMethod.IsReturnTask()
                        ? serviceMethod.ReturnType.GetGenericArguments().First()
                        : serviceMethod.ReturnType;

                var cacheKey = _keyGenerator.GetCacheKey(serviceMethod, invocation.Arguments, attribute.CacheKeyPrefix);

                object cacheValue = null;
                var isAvailable = true;
                try
                {
                    cacheValue = (_cacheProvider.GetAsync(cacheKey, returnType)).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    if (!attribute.IsHightAvailability)
                    {
                        throw;
                    }
                    else
                    {
                        isAvailable = false;
                        _logger?.LogError(new EventId(), ex, $"Cache provider \"{_cacheProvider.Name}\" get error.");
                    }
                }

                if (cacheValue != null)
                {
                    if (serviceMethod.IsReturnTask())
                    {
                        invocation.ReturnValue =
                            TypeofTaskResultMethod.GetOrAdd(returnType, t => typeof(Task).GetMethods().First(p => p.Name == "FromResult" && p.ContainsGenericParameters).MakeGenericMethod(returnType)).Invoke(null, new object[] { cacheValue });
                    }
                    else
                    {
                        invocation.ReturnValue = cacheValue;
                    }
                }
                else
                {
                    // Invoke the method if we don't have a cache hit                    
                    invocation.Proceed();

                    if (!string.IsNullOrWhiteSpace(cacheKey) && invocation.ReturnValue != null && isAvailable)
                    {
                        if (serviceMethod.IsReturnTask())
                        {
                            //get the result
                            var returnValue = invocation.UnwrapAsyncReturnValue().Result;

                            _cacheProvider.Set(cacheKey, returnValue, TimeSpan.FromSeconds(attribute.Expiration));
                        }
                        else
                        {
                            _cacheProvider.Set(cacheKey, invocation.ReturnValue, TimeSpan.FromSeconds(attribute.Expiration));
                        }

                    }

                }
            }
            else
            {
                // Invoke the method if we don't have EasyCachingAbleAttribute
                invocation.Proceed();
            }
        }

        /// <summary>
        /// Processes the put.
        /// </summary>
        /// <param name="invocation">Invocation.</param>
        private void ProcessPut(IInvocation invocation)
        {
            var serviceMethod = invocation.Method ?? invocation.MethodInvocationTarget;

            if (GetMethodAttributes(serviceMethod).FirstOrDefault(x => x.GetType() == typeof(EasyCachingPutAttribute)) is EasyCachingPutAttribute attribute && invocation.ReturnValue != null)
            {
                var cacheKey = _keyGenerator.GetCacheKey(serviceMethod, invocation.Arguments, attribute.CacheKeyPrefix);

                try
                {
                    if (serviceMethod.IsReturnTask())
                    {
                        //get the result
                        var returnValue = invocation.UnwrapAsyncReturnValue().Result;

                        _cacheProvider.Set(cacheKey, returnValue, TimeSpan.FromSeconds(attribute.Expiration));
                    }
                    else
                    {
                        _cacheProvider.Set(cacheKey, invocation.ReturnValue, TimeSpan.FromSeconds(attribute.Expiration));
                    }
                }
                catch (Exception ex)
                {
                    if (!attribute.IsHightAvailability) throw;
                    else _logger?.LogError(new EventId(), ex, $"Cache provider \"{_cacheProvider.Name}\" set error.");
                }
            }
        }

        /// <summary>
        /// Processes the evict.
        /// </summary>
        /// <param name="invocation">Invocation.</param>
        /// <param name="isBefore">If set to <c>true</c> is before.</param>
        private void ProcessEvict(IInvocation invocation, bool isBefore)
        {
            var serviceMethod = invocation.Method ?? invocation.MethodInvocationTarget;

            if (GetMethodAttributes(serviceMethod).FirstOrDefault(x => x.GetType() == typeof(EasyCachingEvictAttribute)) is EasyCachingEvictAttribute attribute && attribute.IsBefore == isBefore)
            {
                try
                {
                    if (attribute.IsAll)
                    {
                        //If is all , clear all cached items which cachekey start with the prefix.
                        var cacheKeyPrefix = _keyGenerator.GetCacheKeyPrefix(serviceMethod, attribute.CacheKeyPrefix);

                        _cacheProvider.RemoveByPrefix(cacheKeyPrefix);
                    }
                    else
                    {
                        //If not all , just remove the cached item by its cachekey.
                        var cacheKey = _keyGenerator.GetCacheKey(serviceMethod, invocation.Arguments, attribute.CacheKeyPrefix);

                        _cacheProvider.Remove(cacheKey);
                    }
                }
                catch (Exception ex)
                {
                    if (!attribute.IsHightAvailability) throw;
                    else _logger?.LogError(new EventId(), ex, $"Cache provider \"{_cacheProvider.Name}\" remove error.");
                }
            }
        }
    }
}
