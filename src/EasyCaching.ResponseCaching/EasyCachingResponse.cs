namespace EasyCaching.ResponseCaching
{    
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// EasyCaching response.
    /// </summary>
    [Serializable]
    internal class EasyCachingResponse
    {
        public DateTimeOffset Created { get; set; }

        public int StatusCode { get; set; }

        public Dictionary<string, string[]> Headers { get; set; }

        public byte[] Body { get; set; }
    }
}