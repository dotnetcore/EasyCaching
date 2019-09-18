using EasyCaching.Core.Interceptor;
using System;

namespace EasyCaching.UnitTests.CustomInterceptors
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    class CustomCachingPutAttribute : EasyCachingPutAttribute
    {
        public CustomCachingPutAttribute()
        {
            Expiration = 1;
            CacheKeyPrefix = "Custom";
        }
    }
}
