namespace EasyCaching.Core
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Easycaching manager.
    /// </summary>
    public class EasyCachingManager
    {
        /// <summary>
        /// The caching providers.
        /// </summary>
        private readonly IEasyCachingProvider[] _cachingProviders;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.Core.EasyCachingManager"/> class.
        /// </summary>
        /// <param name="cachingProviders">Caching providers.</param>
        public EasyCachingManager(IEasyCachingProvider[] cachingProviders)
        {
            if (cachingProviders == null || cachingProviders.Length <= 0)
            {
                throw new ArgumentNullException(nameof(cachingProviders));
            }

            this._cachingProviders = cachingProviders;
        }

        /// <summary>
        /// Get cacheValue by specified cacheKey.
        /// </summary>
        /// <returns>The cacheValue.</returns>
        /// <param name="cacheKey">Cache key.</param>
        public object Get(string cacheKey)
        {
            if (string.IsNullOrWhiteSpace(cacheKey))
            {
                throw new ArgumentNullException(nameof(cacheKey));
            }

            object result = null;

            var missed = new HashSet<int>();

            for (int i = 0; i < _cachingProviders.Length; i++)
            {
                var cachingProvider = _cachingProviders[i];

                var cacheValue = cachingProvider.Get(cacheKey);

                if (cacheValue != null)
                {
                    result = cacheValue;
                    break;
                }
                else
                {
                    missed.Add(i);
                }
            }

            if (result == null)
            {
                result = new EmptyCachingObject();
            }

            //handle missed cache
            foreach (var item in missed)
            {
                var cachingProvider = _cachingProviders[item];

                var cacheEntry = new CacheEntry(cacheKey,
                                                result,
                                                result.GetType().Equals(typeof(EmptyCachingObject))
                                                    ? TimeSpan.FromSeconds(120)
                                                    : TimeSpan.FromSeconds(3600 + new Random().Next(1, 120)));
                cachingProvider.Set(cacheEntry);
            }

            return result;
        }

        /// <summary>
        /// Set the specified cacheKey, cacheValue and absoluteExpirationRelativeToNow.
        /// </summary>
        /// <returns>The set.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cacheValue">Cache value.</param>
        /// <param name="absoluteExpirationRelativeToNow">Absolute expiration relative to now.</param>
        public void Set(string cacheKey, object cacheValue, TimeSpan absoluteExpirationRelativeToNow)
        {
            var cacheEntry = new CacheEntry(cacheKey, cacheValue, absoluteExpirationRelativeToNow);
            this.Set(cacheEntry);
        }

        /// <summary>
        /// Set the specified cacheEntry.
        /// </summary>
        /// <returns>The set.</returns>
        /// <param name="cacheEntry">Cache entry.</param>
        public void Set(CacheEntry cacheEntry)
        {
            for (int i = 0; i < _cachingProviders.Length; i++)
            {
                var cachingProvider = _cachingProviders[i];

                cacheEntry.AbsoluteExpirationRelativeToNow.Add(TimeSpan.FromSeconds(new Random().Next(1, 120)));

                cachingProvider.Set(cacheEntry);
            }
        }

    }
}