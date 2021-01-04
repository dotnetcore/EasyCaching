namespace EasyCaching.LiteDB
{
    public class CacheItem
    {
        public string cachekey { get; set; }
        public string name { get; set; }
        public string cachevalue { get; set; }
        public long expiration { get; set; }
    }
}