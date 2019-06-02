namespace EasyCaching.InMemory
{
    using EasyCaching.Core;
    using EasyCaching.Core.Configurations;

    public class InMemoryOptions : BaseProviderOptions
    {
        public InMemoryOptions()
        {

        }

        public InMemoryCachingOptions DBConfig { get; set; } = new InMemoryCachingOptions();
    }
}
