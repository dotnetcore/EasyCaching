namespace EasyCaching.InMemory
{
    using Microsoft.Extensions.Options;
    using System;

    public class InMemoryCachingOptions : IOptions<InMemoryCachingOptions>
    {
        /// <summary>
        /// Gets or sets the expiration scan frequency.
        /// </summary>
        /// <value>The expiration scan frequency.</value>
        public TimeSpan ExpirationScanFrequency { get; set; } = TimeSpan.FromMinutes(1);

        /// <summary>
        /// Gets or sets the size limit.
        /// </summary>
        /// <value>The size limit.</value>
        public int SizeLimit { get; set; } = 10000;


        InMemoryCachingOptions IOptions<InMemoryCachingOptions>.Value
        {
            get { return this; }
        }
    }
}
