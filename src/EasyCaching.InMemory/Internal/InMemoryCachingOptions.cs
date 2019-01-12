namespace EasyCaching.InMemory
{
    using Microsoft.Extensions.Options;
    using System;

    public class InMemoryCachingOptions : IOptions<InMemoryCachingOptions>
    {
        public TimeSpan ExpirationScanFrequency { get; set; } = TimeSpan.FromMinutes(1);

        public int SizeLimit { get; set; } = 10000;


        InMemoryCachingOptions IOptions<InMemoryCachingOptions>.Value
        {
            get { return this; }
        }
    }
}
