namespace EasyCaching.InMemory
{
    using EasyCaching.Core;
    using EasyCaching.Core.Configurations;

    public class InMemoryOptions : BaseProviderOptionsWithDecorator<IEasyCachingProvider>
    {
        public InMemoryCachingOptions DBConfig { get; set; } = new InMemoryCachingOptions();
    }
}
