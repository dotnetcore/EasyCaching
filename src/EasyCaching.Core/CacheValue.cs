namespace EasyCaching.Core
{
    using System;

    /// <summary>
    /// Cache value.
    /// </summary>
    public class CacheValue<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.Core.CacheValue`1"/> class.
        /// </summary>
        /// <param name="value">Value.</param>
        /// <param name="hasValue">If set to <c>true</c> has value.</param>
        public CacheValue(T value, bool hasValue, TimeSpan? ts = null)
        {
            Value = value;
            HasValue = hasValue;

            if(ts.HasValue)
            {
                ExpiresAt = DateTimeOffset.UtcNow.Add(ts.Value);
            }

        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:EasyCaching.Core.CacheValue`1"/> has value.
        /// </summary>
        /// <value><c>true</c> if has value; otherwise, <c>false</c>.</value>
        public bool HasValue { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:EasyCaching.Core.CacheValue`1"/> is null.
        /// </summary>
        /// <value><c>true</c> if is null; otherwise, <c>false</c>.</value>
        public bool IsNull => Value == null;

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>The value.</value>
        public T Value { get; }

        /// <summary>
        /// Gets or sets the expires at.
        /// </summary>
        /// <value>The expires at.</value>
        public DateTimeOffset ExpiresAt { get; set; }

        /// <summary>
        /// Gets the null.
        /// </summary>
        /// <value>The null.</value>
        public static CacheValue<T> Null { get; } = new CacheValue<T>(default(T), true);

        /// <summary>
        /// Gets the no value.
        /// </summary>
        /// <value>The no value.</value>
        public static CacheValue<T> NoValue { get; } = new CacheValue<T>(default(T), false);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Value?.ToString() ?? "<null>";
        }
    }
}
