namespace EasyCaching.Core.Decoration
{
    using System;

    /// <summary>
    /// Delegate for decoration of caching provider factory
    /// </summary>
    /// <param name="name">Caching provider name</param>
    /// <param name="serviceProvider">Service provider for dependency injection</param>
    /// <param name="cachingProviderFactory">Initial caching provider factory</param>
    /// <typeparam name="TProvider">Caching provider interface (IEasyCachingProvider, IHybridCachingProvider or IRedisAndEasyCachingProvider)</typeparam>
    /// <returns>Decorated caching provider factory</returns>
    public delegate Func<TProvider> ProviderFactoryDecorator<TProvider>(
        string name, IServiceProvider serviceProvider, Func<TProvider> cachingProviderFactory);
    
    public static class CachingProviderDecorationExtensions
    {
        public static TProvider CreateDecoratedProvider<TProvider>(
            this IProviderOptionsWithDecorator<TProvider> options,
            string name,
            IServiceProvider serviceProvider,
            Func<TProvider> cachingProviderFactory) where TProvider : class, IEasyCachingProviderBase
        {
            if (options.ProviderFactoryDecorator == null)
            {
                return cachingProviderFactory();
            }
            else
            {
                var decoratedProviderFactory = options.ProviderFactoryDecorator(name, serviceProvider, cachingProviderFactory);
                return decoratedProviderFactory();
            }
        }
        
        public static IProviderOptionsWithDecorator<TProvider> Decorate<TProvider>(
            this IProviderOptionsWithDecorator<TProvider> options,
            ProviderFactoryDecorator<TProvider> factoryDecorator) where TProvider : class, IEasyCachingProviderBase
        {
            if (options.ProviderFactoryDecorator == null)
            {
                options.ProviderFactoryDecorator = factoryDecorator;
            }
            else
            {
                var existingFactoryDecorator = options.ProviderFactoryDecorator;
                options.ProviderFactoryDecorator = (name, serviceProvider, cachingProviderFactory) =>
                {
                    var factoryDecoratedWithExistingDecorator = existingFactoryDecorator(name, serviceProvider, cachingProviderFactory);
                    return factoryDecorator(name, serviceProvider, factoryDecoratedWithExistingDecorator);
                };
            }

            return options;
        }
    }
}