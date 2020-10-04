namespace EasyCaching.LiteDB
{
    using EasyCaching.Core;
    using EasyCaching.Core.Configurations;

    public class LiteDBOptions: BaseProviderOptions
    {
        public LiteDBOptions()
        {

        }

        public LiteDBDBOptions DBConfig { get; set; } = new LiteDBDBOptions();
    }
}
