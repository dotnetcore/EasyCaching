namespace EasyCaching.InMemory
{
    using EasyCaching.Core;
    using EasyCaching.Core.Configurations;

    public class InMemoryOptions : BaseProviderOptions
    {
        public InMemoryOptions()
        {
            this.CachingProviderType = CachingProviderType.InMemory;
        }

        public InMemoryCachingOptions DBConfig { get; set; } = new InMemoryCachingOptions();
    }
}
