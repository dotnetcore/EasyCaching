namespace EasyCaching.Core.Decoration
{
    using System;

    public interface IProviderOptionsWithDecorator<TProvider>
        where TProvider : IEasyCachingProviderBase
    {
        ProviderFactoryDecorator<TProvider> ProviderFactoryDecorator { get; set; }
    }
}