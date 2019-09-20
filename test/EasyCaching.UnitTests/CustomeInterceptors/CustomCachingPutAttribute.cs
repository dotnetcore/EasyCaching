namespace EasyCaching.UnitTests.CustomInterceptors
{
    using EasyCaching.Core.Interceptor;
    using System;

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
