namespace EasyCaching.Memcached
{
    using Enyim.Caching;
    using Enyim.Caching.Configuration;
    using Microsoft.Extensions.Logging;

    public class EasyCachingMemcachedClient : MemcachedClient
    {
        private readonly string _name;

        public string Name { get { return this._name; } }

        public EasyCachingMemcachedClient(string name, ILoggerFactory loggerFactor, IMemcachedClientConfiguration configuration) 
            : base(loggerFactor, configuration)
        {
            this._name = name;
        }
    }
}
