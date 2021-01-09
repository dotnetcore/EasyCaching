namespace EasyCaching.Core.Decoration
{
    using EasyCaching.Core;

    public class DecoratedHybridCachingProvider : DecoratedEasyCachingProviderBase<IHybridCachingProvider>, IHybridCachingProvider
    {
        public DecoratedHybridCachingProvider(
            string name,
            IEasyCachingProviderDecorator<IHybridCachingProvider> decorator) : base(name, decorator)
        {
        }
    }
}