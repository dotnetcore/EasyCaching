namespace EasyCaching.ResponseCaching
{
    using EasyCaching.Core;
    using EasyCaching.Core.Internal;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.ResponseCaching.Internal;
    using Microsoft.Extensions.Primitives;
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// EasyCaching response cache.
    /// </summary>
    public class EasyCachingResponseCache : IResponseCache
    {
        /// <summary>
        /// The provider.
        /// </summary>
        private readonly IEasyCachingProvider _provider;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.ResponseCaching.EasyCachingResponseCache"/> class.
        /// </summary>
        /// <param name="provider">Provider.</param>
        public EasyCachingResponseCache(
            IEasyCachingProvider provider)
        {
            ArgumentCheck.NotNull(provider, nameof(provider));

            _provider = provider;
        }

        /// <summary>
        /// Get the specified key.
        /// </summary>
        /// <returns>The get.</returns>
        /// <param name="key">Key.</param>
        public IResponseCacheEntry Get(string key)
        {
            var entry = _provider.Get<IResponseCacheEntry>(key);

            if (entry.HasValue)
            {
                var val = entry.Value as EasyCachingResponse;
                if (val != null)
                {
                    return new CachedResponse
                    {
                        Created = val.Created,
                        StatusCode = val.StatusCode,
                        Headers = new HeaderDictionary(val.Headers.ToDictionary(x => x.Key, x => new StringValues(x.Value))),
                        Body = new MemoryStream(val.Body)
                    };
                }
            }

            return entry as IResponseCacheEntry;
        }

        /// <summary>
        /// Gets the async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="key">Key.</param>
        public async Task<IResponseCacheEntry> GetAsync(string key)
        {
            var entry = await _provider.GetAsync<IResponseCacheEntry>(key);

            if (entry.HasValue)
            {
                var val = entry.Value as EasyCachingResponse;
                if (val != null)
                {
                    return new CachedResponse
                    {
                        Created = val.Created,
                        StatusCode = val.StatusCode,
                        Headers = new HeaderDictionary(val.Headers.ToDictionary(x => x.Key, x => new StringValues(x.Value))),
                        Body = new MemoryStream(val.Body)
                    };
                }
            }

            return entry as IResponseCacheEntry;
        }

        /// <summary>
        /// Set the specified key, entry and validFor.
        /// </summary>
        /// <returns>The set.</returns>
        /// <param name="key">Key.</param>
        /// <param name="entry">Entry.</param>
        /// <param name="validFor">Valid for.</param>
        public void Set(string key, IResponseCacheEntry entry, TimeSpan validFor)
        {
            var cachedResponse = entry as CachedResponse;
            if (cachedResponse != null)
            {
                _provider.Set(
                    key,
                    new EasyCachingResponse
                    {
                        Created = cachedResponse.Created,
                        StatusCode = cachedResponse.StatusCode,
                        Headers = cachedResponse.Headers.ToDictionary(x => x.Key, x => x.Value.ToArray()),
                        Body = this.GetBytes(cachedResponse.Body)
                    },
                    validFor);
            }
            else
            {
                _provider.Set(
                    key,
                    entry,
                    validFor);
            }
        }

        /// <summary>
        /// Sets the async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="key">Key.</param>
        /// <param name="entry">Entry.</param>
        /// <param name="validFor">Valid for.</param>
        public async Task SetAsync(string key, IResponseCacheEntry entry, TimeSpan validFor)
        {
            var cachedResponse = entry as CachedResponse;
            if (cachedResponse != null)
            {
                await _provider.SetAsync(
                    key,
                    new EasyCachingResponse
                    {
                        Created = cachedResponse.Created,
                        StatusCode = cachedResponse.StatusCode,
                        Headers = cachedResponse.Headers.ToDictionary(x => x.Key, x => x.Value.ToArray()),
                        Body = this.GetBytes(cachedResponse.Body)
                    },
                    validFor);
            }
            else
            {
                await _provider.SetAsync(
                    key,
                    entry,
                    validFor);
            }
        }

        /// <summary>
        /// Gets the bytes.
        /// </summary>
        /// <returns>The bytes.</returns>
        /// <param name="stream">Stream.</param>
        private byte[] GetBytes(Stream stream)
        {
            int responseLengthInBytes = (int)stream.Length;
            long oldPosition = stream.Position;
            stream.Position = 0;
            byte[] body = new byte[responseLengthInBytes];
            stream.Read(body, 0, responseLengthInBytes);
            stream.Position = oldPosition;

            return body;
        }
    }
}