namespace EasyCaching.CSRedis
{
    using EasyCaching.Core;
    using EasyCaching.Core.Internal;
    using global::CSRedis;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public partial class DefaultCSRedisCachingProvider : IRedisCachingProvider 
    {
        public bool HSet<T>(string cacheKey, string  field, T cacheValue, TimeSpan? expiration = null)
        {
            if(expiration.HasValue)
            {
                var script = @"
            local r = redis.call('HSET', KEYS[1], ARGV[1], ARGV[2])

            if tonumber(r) == 1 then 
               redis.call('EXPIRE', KEYS[1], tonumber(ARGV[3])) 
               return 1
            else
               return 0 
            end
            ";
                var res = (int)_cache.Eval(script, cacheKey, field, _serializer.Serialize(cacheValue), expiration.Value.Seconds);
                return res == 1;
            }
            else
            {
                return _cache.HSet(cacheKey, field, _serializer.Serialize(cacheValue));
            }
        }              
    }
}
