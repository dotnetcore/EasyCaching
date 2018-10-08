namespace EasyCaching.Core
{
    using System.Collections.Generic;
    using System.Linq;

    public interface IEasyCachingProviderFactory
    {
        IEasyCachingProvider GetCachingProvider(string name);
    }

    public class DefaultEasyCachingProviderFactory : IEasyCachingProviderFactory
    {
        private readonly IEnumerable<IEasyCachingProvider> _cachingProviders;

        public DefaultEasyCachingProviderFactory(IEnumerable<IEasyCachingProvider> cachingProviders)
        {
            this._cachingProviders = cachingProviders;
        }

        public IEasyCachingProvider GetCachingProvider(string name)
        {
            Internal.ArgumentCheck.NotNullOrWhiteSpace(name, nameof(name));

            var provider = _cachingProviders.FirstOrDefault(x => x.Name.Equals(name));

            if (provider == null) throw new System.ArgumentException("can not find a match caching provider!");

            return provider;
        }
    }
}
