namespace EasyCaching.ResponseCaching
{
    using EasyCaching.Core;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// EasyCaching response cache.
    /// </summary>
    internal class EasyCachingResponseCache : IResponseCache
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
        internal EasyCachingResponseCache(string name,
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
            var entry = _provider.Get<object>(key);

            if (entry.HasValue)
            {
                if (entry.Value is EasyCachingResponse val)
                {
                    return new CachedResponse
                    {
                        Created = val.Created,
                        StatusCode = val.StatusCode,
                        Headers = val.Headers,
                        Body = new SegmentReadStream(val.BodySegments, val.BodyLength)
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
            var entry = await _provider.GetAsync<object>(key);

            if (entry.HasValue)
            {
                if (entry.Value is EasyCachingResponse val)
                {
                    return new CachedResponse
                    {
                        Created = val.Created,
                        StatusCode = val.StatusCode,
                        Headers = val.Headers,
                        Body = new SegmentReadStream(val.BodySegments, val.BodyLength)
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
                var segmentStream = new SegmentWriteStream(StreamUtilities.BodySegmentSize);
                cachedResponse.Body.CopyTo(segmentStream);

                _provider.Set(
                    key,
                    new EasyCachingResponse
                    {
                        Created = cachedResponse.Created,
                        StatusCode = cachedResponse.StatusCode,
                        Headers = cachedResponse.Headers,
                        BodySegments = segmentStream.GetSegments(),
                        BodyLength = segmentStream.Length
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
                var segmentStream = new SegmentWriteStream(StreamUtilities.BodySegmentSize);
                cachedResponse.Body.CopyTo(segmentStream);

                await _provider.SetAsync(
                    key,
                    new EasyCachingResponse
                    {
                        Created = cachedResponse.Created,
                        StatusCode = cachedResponse.StatusCode,
                        Headers = cachedResponse.Headers,
                        BodySegments = segmentStream.GetSegments(),
                        BodyLength = segmentStream.Length
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
    }
}