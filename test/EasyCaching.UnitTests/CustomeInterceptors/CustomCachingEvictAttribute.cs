namespace EasyCaching.UnitTests.CustomInterceptors
{
    using EasyCaching.Core.Interceptor;
    using System;

    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    class CustomCachingEvictAttribute : EasyCachingEvictAttribute
    {
        public CustomCachingEvictAttribute()
        {
            CacheKeyPrefix = "Custom";
        }
    }
}
