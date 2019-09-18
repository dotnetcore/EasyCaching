using EasyCaching.Core.Interceptor;
using System;

namespace EasyCaching.UnitTests.CustomInterceptors
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    class CustomCachingAbleAttribute: EasyCachingAbleAttribute
    {
        public CustomCachingAbleAttribute()
        {
            Expiration = 1;
            CacheKeyPrefix = "Custom";
        }
    }
}
