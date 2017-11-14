namespace EasyCaching.Core
{
    using System;

    /// <summary>
    /// Easycaching attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class EasyCachingAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the absolute expiration relative to now.
        /// </summary>
        /// <example>
        /// 30 sec.
        /// </example>
        /// <value>The absolute expiration relative to now.</value>
        public long AbsoluteExpirationRelativeToNow { get; set; } = 30;
    }
}
