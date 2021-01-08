namespace EasyCaching.Core.Configurations
{
    using Decoration;

    public class BaseProviderOptionsWithDecorator<TProvider> : BaseProviderOptions,
        IProviderOptionsWithDecorator<TProvider>
        where TProvider : IEasyCachingProviderBase
    {
        public EasyCachingProviderDecoratorFactory<TProvider> ProviderDecoratorFactory { get; set; }
    }
}