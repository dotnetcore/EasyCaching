namespace EasyCaching.ResponseCaching
{
    using System;
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// EasyCaching response.
    /// </summary>
    [Serializable]
    public class EasyCachingResponse
    {
        public DateTimeOffset Created { get; set; }

        public int StatusCode { get; set; }

        public IHeaderDictionary Headers { get; set; } = new HeaderDictionary();

        public List<byte[]> BodySegments { get; set; }

        public long BodyLength { get; set; }
    }
}