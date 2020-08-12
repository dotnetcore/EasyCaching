namespace Microsoft.Extensions.Caching.EasyCachingLib
{
    using EasyCaching.Core;
    using Microsoft.Extensions.Caching.Distributed;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public class EasyCachingRedisCache : IDistributedCache
    {
        // KEYS[1] = = key
        // ARGV[1] = absolute-expiration - ticks as long (-1 for none)
        // ARGV[2] = sliding-expiration - ticks as long (-1 for none)
        // ARGV[3] = relative-expiration (long, in seconds, -1 for none) - Min(absolute-expiration - Now, sliding-expiration)
        // ARGV[4] = data - byte[]
        // this order should not change LUA script depends on it
        private const string SetScript = (@"
                redis.call('HMSET', KEYS[1], 'absexp', ARGV[1], 'sldexp', ARGV[2], 'data', ARGV[4])
                if ARGV[3] ~= '-1' then
                  redis.call('EXPIRE', KEYS[1], ARGV[3])
                end
                return 1");
        private const string AbsoluteExpirationKey = "absexp";
        private const string SlidingExpirationKey = "sldexp";
        private const string DataKey = "data";
        private const long NotPresent = -1;

        private readonly IRedisCachingProvider _provider;

        public EasyCachingRedisCache(IRedisCachingProvider provider)
        {
            _provider = provider;
        }

        public byte[] Get(string key)
        {
            ArgumentCheck.NotNull(key, nameof(key));

            return GetAndRefresh(key, getData: true);
        }

        public async Task<byte[]> GetAsync(string key, CancellationToken token = default)
        {
            ArgumentCheck.NotNull(key, nameof(key));

            token.ThrowIfCancellationRequested();

            return await GetAndRefreshAsync(key, getData: true, token: token);
        }

        public void Refresh(string key)
        {
            ArgumentCheck.NotNull(key, nameof(key));

            GetAndRefresh(key, getData: false);
        }

        public async Task RefreshAsync(string key, CancellationToken token = default)
        {
            ArgumentCheck.NotNull(key, nameof(key));

            token.ThrowIfCancellationRequested();

            await GetAndRefreshAsync(key, getData: false, token: token);
        }

        public void Remove(string key)
        {
            ArgumentCheck.NotNull(key, nameof(key));

            _provider.KeyDel(key);
        }

        public async Task RemoveAsync(string key, CancellationToken token = default)
        {
            ArgumentCheck.NotNull(key, nameof(key));

            await _provider.KeyDelAsync(key);
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            ArgumentCheck.NotNull(key, nameof(key));
            ArgumentCheck.NotNull(value, nameof(value));
            ArgumentCheck.NotNull(options, nameof(options));

            var creationTime = DateTimeOffset.UtcNow;

            var absoluteExpiration = GetAbsoluteExpiration(creationTime, options);

            var list = new List<object>
            {
                absoluteExpiration?.Ticks ?? NotPresent,
                options.SlidingExpiration?.Ticks ?? NotPresent,
                GetExpirationInSeconds(creationTime, absoluteExpiration, options) ?? NotPresent,

                // Hash in RedisProvider use string, not a good idea here.
                System.Text.Encoding.UTF8.GetString(value)
            };

            var result = _provider.Eval(SetScript, key, list);
        }

        public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            ArgumentCheck.NotNull(key, nameof(key));
            ArgumentCheck.NotNull(value, nameof(value));
            ArgumentCheck.NotNull(options, nameof(options));

            var creationTime = DateTimeOffset.UtcNow;

            var absoluteExpiration = GetAbsoluteExpiration(creationTime, options);

            var list = new List<object>
            {
                absoluteExpiration?.Ticks ?? NotPresent,
                options.SlidingExpiration?.Ticks ?? NotPresent,
                GetExpirationInSeconds(creationTime, absoluteExpiration, options) ?? NotPresent,
                
                // Hash in RedisProvider use string, not a good idea here.
                System.Text.Encoding.UTF8.GetString(value)
            };

            var result = await _provider.EvalAsync(SetScript, key, list);
        }

        private byte[] GetAndRefresh(string key, bool getData)
        {
            ArgumentCheck.NotNull(key, nameof(key));

            object[] results;
            byte[] value = null;

            if (getData)
            {
                var dict = _provider.HMGet(key, new List<string> { AbsoluteExpirationKey, SlidingExpirationKey, DataKey });

                var first = dict[AbsoluteExpirationKey]?.ToString();
                var second = dict[SlidingExpirationKey]?.ToString();
                value = System.Text.Encoding.UTF8.GetBytes(dict[DataKey]);

                results = new object[] { first, second, value };
            }
            else
            {
                var dict = _provider.HMGet(key, new List<string> { AbsoluteExpirationKey, SlidingExpirationKey });

                var first = dict[AbsoluteExpirationKey]?.ToString();
                var second = dict[SlidingExpirationKey]?.ToString();

                results = new object[] { first, second };
            }

            if (results.Length >= 2)
            {
                MapMetadata(results, out DateTimeOffset? absExpr, out TimeSpan? sldExpr);
                Refresh(key, absExpr, sldExpr);
            }

            if (results.Length >= 3)
            {
                return value;
            }

            return null;
        }

        private async Task<byte[]> GetAndRefreshAsync(string key, bool getData, CancellationToken token = default(CancellationToken))
        {
            ArgumentCheck.NotNull(key, nameof(key));

            token.ThrowIfCancellationRequested();

            object[] results;
            byte[] value = null;
            if (getData)
            {
                var dict = await _provider.HMGetAsync(key, new List<string> { AbsoluteExpirationKey, SlidingExpirationKey, DataKey });

                var first = dict[AbsoluteExpirationKey]?.ToString();
                var second = dict[SlidingExpirationKey]?.ToString();
                value = System.Text.Encoding.UTF8.GetBytes(dict[DataKey]);

                results = new object[] { first, second, value };
            }
            else
            {
                var dict = await _provider.HMGetAsync(key, new List<string> { AbsoluteExpirationKey, SlidingExpirationKey });

                var first = dict[AbsoluteExpirationKey]?.ToString();
                var second = dict[SlidingExpirationKey]?.ToString();

                results = new object[] { first, second };
            }

            if (results.Length >= 2)
            {
                MapMetadata(results, out DateTimeOffset? absExpr, out TimeSpan? sldExpr);
                await RefreshAsync(key, absExpr, sldExpr, token);
            }

            if (results.Length >= 3)
            {
                return value;
            }

            return null;
        }

        private void MapMetadata(object[] results, out DateTimeOffset? absoluteExpiration, out TimeSpan? slidingExpiration)
        {
            absoluteExpiration = null;
            slidingExpiration = null;
            if (long.TryParse(results[0]?.ToString(), out var absoluteExpirationTicks) && absoluteExpirationTicks != NotPresent)
            {
                absoluteExpiration = new DateTimeOffset(absoluteExpirationTicks, TimeSpan.Zero);
            }
            if (long.TryParse(results[1]?.ToString(), out var slidingExpirationTicks) && slidingExpirationTicks != NotPresent)
            {
                slidingExpiration = new TimeSpan(slidingExpirationTicks);
            }
        }

        private void Refresh(string key, DateTimeOffset? absExpr, TimeSpan? sldExpr)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            TimeSpan? expr = null;
            if (sldExpr.HasValue)
            {
                if (absExpr.HasValue)
                {
                    var relExpr = absExpr.Value - DateTimeOffset.Now;
                    expr = relExpr <= sldExpr.Value ? relExpr : sldExpr;
                }
                else
                {
                    expr = sldExpr;
                }

                _provider.KeyExpire(key, (int)(expr ?? TimeSpan.Zero).TotalSeconds);
            }
        }

        private async Task RefreshAsync(string key, DateTimeOffset? absExpr, TimeSpan? sldExpr, CancellationToken token = default(CancellationToken))
        {
            ArgumentCheck.NotNull(key, nameof(key));

            token.ThrowIfCancellationRequested();

            TimeSpan? expr = null;
            if (sldExpr.HasValue)
            {
                if (absExpr.HasValue)
                {
                    var relExpr = absExpr.Value - DateTimeOffset.Now;
                    expr = relExpr <= sldExpr.Value ? relExpr : sldExpr;
                }
                else
                {
                    expr = sldExpr;
                }

                await _provider.KeyExpireAsync(key, (int)(expr ?? TimeSpan.Zero).TotalSeconds);
            }
        }

        private static long? GetExpirationInSeconds(DateTimeOffset creationTime, DateTimeOffset? absoluteExpiration, DistributedCacheEntryOptions options)
        {
            if (absoluteExpiration.HasValue && options.SlidingExpiration.HasValue)
            {
                return (long)Math.Min(
                    (absoluteExpiration.Value - creationTime).TotalSeconds,
                    options.SlidingExpiration.Value.TotalSeconds);
            }
            else if (absoluteExpiration.HasValue)
            {
                return (long)(absoluteExpiration.Value - creationTime).TotalSeconds;
            }
            else if (options.SlidingExpiration.HasValue)
            {
                return (long)options.SlidingExpiration.Value.TotalSeconds;
            }
            return null;
        }

        private static DateTimeOffset? GetAbsoluteExpiration(DateTimeOffset creationTime, DistributedCacheEntryOptions options)
        {
            if (options.AbsoluteExpiration.HasValue && options.AbsoluteExpiration <= creationTime)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(DistributedCacheEntryOptions.AbsoluteExpiration),
                    options.AbsoluteExpiration.Value,
                    "The absolute expiration value must be in the future.");
            }
            var absoluteExpiration = options.AbsoluteExpiration;
            if (options.AbsoluteExpirationRelativeToNow.HasValue)
            {
                absoluteExpiration = creationTime + options.AbsoluteExpirationRelativeToNow;
            }

            return absoluteExpiration;
        }
    }
}
