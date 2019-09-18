using EasyCaching.Core.Interceptor;
using System;

namespace EasyCaching.UnitTests.CustomInterceptors
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    class CustomCachingEvictAttribute : EasyCachingEvictAttribute
    {
        public CustomCachingEvictAttribute()
        {
            CacheKeyPrefix = "Custom";
        }
    }
}
