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
        /// <param name="name">Provider Name</param>
        /// <param name="providerFactory">Provider factory</param>
        public EasyCachingResponseCache(string name,
            IEasyCachingProviderFactory providerFactory)
        {
            ArgumentCheck.NotNull(providerFactory, nameof(providerFactory));

            _provider = providerFactory.GetCachingProvider(name);
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
                if (entry.Value is EasyCachingResponse val)
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

            return entry.Value as IResponseCacheEntry;
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
                if (entry.Value is EasyCachingResponse val)
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

            return entry.Value as IResponseCacheEntry;
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
            if (entry is CachedResponse cachedResponse)
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
            if (entry is CachedResponse cachedResponse)
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
            int count = (int)stream.Length;
            long oldPosition = stream.Position;
            stream.Position = 0;
            byte[] body = new byte[count];

            //for HEAD method
            if (count > 0)
            {
                stream.Read(body, 0, count);
                stream.Position = oldPosition;
            }

            return body;
        }
    }
}