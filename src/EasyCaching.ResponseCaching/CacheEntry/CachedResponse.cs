namespace EasyCaching.ResponseCaching
{
    using Microsoft.AspNetCore.Http;
    using System;
    using System.IO;

    internal class CachedResponse : IResponseCacheEntry
    {
        public DateTimeOffset Created { get; set; }

        public int StatusCode { get; set; }

        public IHeaderDictionary Headers { get; set; }

        public Stream Body { get; set; }
    }
}
