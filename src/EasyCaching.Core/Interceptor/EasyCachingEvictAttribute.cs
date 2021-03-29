namespace EasyCaching.Core.Interceptor
{
    using System;

    /// <summary>
    /// Easycaching evict attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public class EasyCachingEvictAttribute : EasyCachingInterceptorAttribute
    {        
        /// <summary>
        /// Gets or sets a value indicating whether evict all cached values by cachekey prefix
        /// </summary>
        /// <remarks>
        /// This need to use with CacheKeyPrefix.
        /// </remarks>
        /// <value><c>true</c> if is all; otherwise, <c>false</c>.</value>
        public bool IsAll { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether is before.
        /// </summary>
        /// <value><c>true</c> if is before; otherwise, <c>false</c>.</value>
        public bool IsBefore { get; set; } = false;
    }
}
