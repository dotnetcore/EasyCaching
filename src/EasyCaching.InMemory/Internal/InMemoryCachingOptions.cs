namespace EasyCaching.InMemory
{
    using Microsoft.Extensions.Options;

    public class InMemoryCachingOptions : IOptions<InMemoryCachingOptions>
    {
        /// <summary>
        /// Gets or sets the expiration scan frequency, the unit is second.
        /// </summary>
        /// <value>The expiration scan frequency.</value>
        public int ExpirationScanFrequency { get; set; } = 60;

        /// <summary>
        /// Gets or sets the size limit.
        /// </summary>
        /// <value>The size limit.</value>
        public int? SizeLimit { get; set; }// = 10000;

        /// <summary>
        /// Gets or sets whether to enable deep clone when reading object from cache.
        /// </summary>
        public bool EnableReadDeepClone { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to enable deep clone when writing object to cache.
        /// </summary>
        public bool EnableWriteDeepClone { get; set; } = false;

        InMemoryCachingOptions IOptions<InMemoryCachingOptions>.Value
        {
            get { return this; }
        }
    }
}
