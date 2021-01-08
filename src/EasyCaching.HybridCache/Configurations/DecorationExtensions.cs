namespace EasyCaching.HybridCache
{
    using Core;
    using Core.Decoration;
    using System;

    public static class DecorationExtensions
    {
        public static IHybridCachingProvider CreateDecoratedProvider(
            this IProviderOptionsWithDecorator<IHybridCachingProvider> options,
            string name,
            IServiceProvider serviceProvider,
            Func<IHybridCachingProvider> cachingProviderFactory) 
        {
            if (options.ProviderDecoratorFactory == null)
            {
                return cachingProviderFactory();
            }
            else
            {
                var decorator = options.ProviderDecoratorFactory(name, serviceProvider, cachingProviderFactory);
                return new HybridCachingProviderDecorator(name, decorator);
            }
        }
    }
        
    public class HybridCachingProviderDecorator : DecoratedEasyCachingProviderBase<IHybridCachingProvider>, IHybridCachingProvider
    {
        public HybridCachingProviderDecorator(
            string name,
            IEasyCachingProviderDecorator<IHybridCachingProvider> decorator) : base(name, decorator)
        {
        }
    }
}