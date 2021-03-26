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
        private readonly IEnumerable<IEasyCachingProvider> _cachingProviders;

        public DefaultEasyCachingProviderFactory(IEnumerable<IEasyCachingProvider> cachingProviders)
        {
            _cachingProviders = cachingProviders;
        }

        public IEasyCachingProvider GetCachingProvider(string name)
        {
            ArgumentCheck.NotNullOrWhiteSpace(name, nameof(name));

            var provider = _cachingProviders.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (provider == null) throw new ArgumentException("can not find a match caching provider!");

            return provider;
        }
    }
}
