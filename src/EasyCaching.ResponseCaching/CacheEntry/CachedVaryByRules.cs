namespace EasyCaching.ResponseCaching
{
    using Microsoft.Extensions.Primitives;

    public class CachedVaryByRules : IResponseCacheEntry
    {
        public string VaryByKeyPrefix { get; set; }

        public StringValues Headers { get; set; }

        public StringValues QueryKeys { get; set; }
    }
}
