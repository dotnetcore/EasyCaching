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
        /// Gets or sets the expiration.
        /// </summary>
        /// <value>The expiration.</value>
        public int Expiration { get; set; } = 30;

        /// <summary>
        /// Gets or sets the parameter count.
        /// </summary>
        /// <value>The parameter count.</value>
        public int ParamCount { get; set; } = 5;
    }
}
