namespace EasyCaching.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Default easycaching hybrid provider factory.
    /// </summary>
    public class DefaultHybridProviderFactory : IHybridProviderFactory
    {      
        private readonly IEnumerable<IHybridCachingProvider> _hybridProviders;

        public DefaultHybridProviderFactory(IEnumerable<IHybridCachingProvider> hybridProviders)
        {
            this._hybridProviders = hybridProviders;
        }

        public IHybridCachingProvider GetHybridCachingProvider(string name)
        {
            ArgumentCheck.NotNullOrWhiteSpace(name, nameof(name));

            var provider = _hybridProviders.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (provider == null) throw new ArgumentException("can not find a match hybrid provider!");

            return provider;
        }
    }
}
