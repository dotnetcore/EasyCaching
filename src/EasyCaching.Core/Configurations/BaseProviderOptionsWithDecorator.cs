namespace EasyCaching.Core.Configurations
{
    using Decoration;

    public class BaseProviderOptionsWithDecorator<TProvider> : BaseProviderOptions,
        IProviderOptionsWithDecorator<TProvider>
        where TProvider : IEasyCachingProviderBase
    {
        public ProviderFactoryDecorator<TProvider> ProviderFactoryDecorator { get; set; }
    }
}