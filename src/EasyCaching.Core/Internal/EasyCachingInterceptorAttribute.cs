namespace EasyCaching.Core.Internal
{
    using System;

    /// <summary>
    /// Easycaching interceptor attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public class EasyCachingInterceptorAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets a value indicating whether is hybrid provider.
        /// </summary>
        /// <value><c>true</c> if is hybrid provider; otherwise, <c>false</c>.</value>
        public bool IsHybridProvider { get; set; } = false;

        /// <summary>
        /// Gets or sets the cache key prefix. 
        /// </summary>
        /// <value>The cache key prefix.</value>
        public string CacheKeyPrefix { get; set; } = string.Empty;
    }

    /// <summary>
    /// Easycaching able attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public class EasyCachingAbleAttribute : EasyCachingInterceptorAttribute
    {
        /// <summary>
        /// Gets or sets the expiration. The default value is 30 second.
        /// </summary>
        /// <value>The expiration.</value>
        public int Expiration { get; set; } = 30;
    }

    /// <summary>
    /// Easycaching put attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public class EasyCachingPutAttribute : EasyCachingInterceptorAttribute
    {
        /// <summary>
        /// Gets or sets the expiration. The default value is 30 second.
        /// </summary>
        /// <value>The expiration.</value>
        public int Expiration { get; set; } = 30;
    }

    /// <summary>
    /// Easycaching evict attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public class EasyCachingEvictAttribute : EasyCachingInterceptorAttribute
    {        
        /// <summary>
        /// Gets or sets a value indicating whether evict all cached values which are 
        /// </summary>
        /// <value><c>true</c> if all; otherwise, <c>false</c>.</value>
        public bool All { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether is before.
        /// </summary>
        /// <value><c>true</c> if is before; otherwise, <c>false</c>.</value>
        public bool IsBefore { get; set; } = false;
    }
}
