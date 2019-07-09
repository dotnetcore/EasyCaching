namespace EasyCaching.Core.Interceptor
{
    using System;

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
}
