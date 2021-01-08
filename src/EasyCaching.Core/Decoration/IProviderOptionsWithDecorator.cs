namespace EasyCaching.Core.Decoration
{
    using System;

    public interface IProviderOptionsWithDecorator<TProvider>
        where TProvider : IEasyCachingProviderBase
    {
        EasyCachingProviderDecoratorFactory<TProvider> ProviderDecoratorFactory { get; set; }
    }

    public delegate IEasyCachingProviderDecorator<TProvider> EasyCachingProviderDecoratorFactory<TProvider>(
            string name, IServiceProvider serviceProvider, Func<TProvider> cachingProviderFactory)
        where TProvider : IEasyCachingProviderBase;
}