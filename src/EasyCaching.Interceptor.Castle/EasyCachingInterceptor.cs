namespace EasyCaching.Interceptor.Castle
{
    using EasyCaching.Core;
    using EasyCaching.Core.Internal;
    using global::Castle.DynamicProxy;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    /// <summary>
    /// Easycaching interceptor.
    /// </summary>
    public class EasyCachingInterceptor : IInterceptor
    {
        /// <summary>
        /// The cache provider.
        /// </summary>
        private readonly IEasyCachingProvider _cacheProvider;

        /// <summary>
        /// The link char.
        /// </summary>
        private char _linkChar = ':';

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.Interceptor.Castle.EasyCachingInterceptor"/> class.
        /// </summary>
        /// <param name="cacheProvider">Cache provider.</param>
        public EasyCachingInterceptor(IEasyCachingProvider cacheProvider)
        {
            _cacheProvider = cacheProvider;
        }

        /// <summary>
        /// Intercept the specified invocation.
        /// </summary>
        /// <returns>The intercept.</returns>
        /// <param name="invocation">Invocation.</param>
        public void Intercept(IInvocation invocation)
        {
            var cachingAttribute = this.GetEasyCachingInterceptorAttributeInfo(invocation.MethodInvocationTarget ?? invocation.Method);
            if (cachingAttribute != null)
            {
                ProceedCaching(invocation, cachingAttribute);
            }
            else
            {
                invocation.Proceed();
            }
        }

        /// <summary>
        /// Gets the easy caching interceptor attribute info.
        /// </summary>
        /// <returns>The easy caching interceptor attribute info.</returns>
        /// <param name="method">Method.</param>
        private EasyCachingInterceptorAttribute GetEasyCachingInterceptorAttributeInfo(MethodInfo method)
        {
            return method.GetCustomAttributes(true).FirstOrDefault(x => x.GetType() == typeof(EasyCachingInterceptorAttribute)) as EasyCachingInterceptorAttribute;
        }

        /// <summary>
        /// Proceeds the caching.
        /// </summary>
        /// <param name="invocation">Invocation.</param>
        /// <param name="attribute">Attribute.</param>
        private void ProceedCaching(IInvocation invocation, EasyCachingInterceptorAttribute attribute)
        {
            var cacheKey = GenerateCacheKey(invocation);

            var cacheValue = _cacheProvider.Get<object>(cacheKey);

            if(cacheValue.HasValue)
            {
                invocation.ReturnValue = cacheValue.Value;
                return;
            }

            invocation.Proceed();

            if (!string.IsNullOrWhiteSpace(cacheKey))
            {
                _cacheProvider.Set(cacheKey, invocation.ReturnValue, TimeSpan.FromSeconds(attribute.Expiration));
            }
        }

        /// <summary>
        /// Generates the cache key.
        /// </summary>
        /// <returns>The cache key.</returns>
        /// <param name="invocation">Invocation.</param>
        private string GenerateCacheKey(IInvocation invocation)
        {
            var typeName = invocation.TargetType.Name;
            var methodName = invocation.Method.Name;
            var methodArguments = this.FormatArgumentsToPartOfCacheKey(invocation.Arguments);

            return this.GenerateCacheKey(typeName, methodName, methodArguments);
        }

        /// <summary>
        /// Generates the cache key.
        /// </summary>
        /// <returns>The cache key.</returns>
        /// <param name="typeName">Type name.</param>
        /// <param name="methodName">Method name.</param>
        /// <param name="parameters">Parameters.</param>
        private string GenerateCacheKey(string typeName, string methodName, IList<string> parameters)
        {
            var builder = new StringBuilder();

            builder.Append(typeName);
            builder.Append(_linkChar);

            builder.Append(methodName);
            builder.Append(_linkChar);

            foreach (var param in parameters)
            {
                builder.Append(param);
                builder.Append(_linkChar);
            }

            return builder.ToString().TrimEnd(_linkChar);
        }

        /// <summary>
        /// Formats the arguments to part of cache key.
        /// </summary>
        /// <returns>The arguments to part of cache key.</returns>
        /// <param name="methodArguments">Method arguments.</param>
        /// <param name="maxCount">Max count.</param>
        private IList<string> FormatArgumentsToPartOfCacheKey(IList<object> methodArguments, int maxCount = 5)
        {
            return methodArguments.Select(this.GetArgumentValue).Take(maxCount).ToList();
        }

        /// <summary>
        /// Gets the argument value.
        /// </summary>
        /// <returns>The argument value.</returns>
        /// <param name="arg">Argument.</param>
        private string GetArgumentValue(object arg)
        {
            if (arg is int || arg is long || arg is string)
                return arg.ToString();

            if (arg is DateTime)
                return ((DateTime)arg).ToString("yyyyMMddHHmmss");

            if (arg is ICachable)
                return ((ICachable)arg).CacheKey;

            return null;
        }
    }
}
