namespace EasyCaching.UnitTests.CustomInterceptors
{
    using EasyCaching.Core.Interceptor;
    using System;

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
