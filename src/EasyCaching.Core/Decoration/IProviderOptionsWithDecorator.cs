namespace EasyCaching.Core.Decoration
{
    using System;

    public interface IProviderOptionsWithDecorator<TProvider>
        where TProvider : IEasyCachingProvider
    {
        ProviderFactoryDecorator<TProvider> ProviderFactoryDecorator { get; set; }
    }
}