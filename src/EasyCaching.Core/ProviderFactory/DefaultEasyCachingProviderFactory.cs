namespace EasyCaching.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Default easycaching provider factory.
    /// </summary>
    public class DefaultEasyCachingProviderFactory : IEasyCachingProviderFactory
    {
        /// <summary>
        /// The caching providers.
        /// </summary>
        private readonly IEnumerable<IEasyCachingProvider> _cachingProviders;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.Core.DefaultEasyCachingProviderFactory"/> class.
        /// </summary>
        /// <param name="cachingProviders">Caching providers.</param>
        public DefaultEasyCachingProviderFactory(IEnumerable<IEasyCachingProvider> cachingProviders)
        {
            this._cachingProviders = cachingProviders;
        }

        /// <summary>
        /// Gets the caching provider.
        /// </summary>
        /// <returns>The caching provider.</returns>
        /// <param name="name">Name.</param>
        public IEasyCachingProvider GetCachingProvider(string name)
        {
            ArgumentCheck.NotNullOrWhiteSpace(name, nameof(name));

            var provider = _cachingProviders.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (provider == null) throw new ArgumentException("can not find a match caching provider!");

            return provider;
        }
    }
}
