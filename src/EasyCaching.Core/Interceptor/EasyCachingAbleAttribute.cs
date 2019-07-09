namespace EasyCaching.Core.Interceptor
{
    using System;

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
}
