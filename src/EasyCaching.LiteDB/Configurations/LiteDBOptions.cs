namespace EasyCaching.LiteDB
{
    using EasyCaching.Core;
    using EasyCaching.Core.Configurations;

    public class LiteDBOptions : BaseProviderOptionsWithDecorator<IEasyCachingProvider>
    {
        public LiteDBDBOptions DBConfig { get; set; } = new LiteDBDBOptions();
    }
}
