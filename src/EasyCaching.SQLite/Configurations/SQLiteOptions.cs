namespace EasyCaching.SQLite
{
    using EasyCaching.Core;
    using EasyCaching.Core.Configurations;

    public class SQLiteOptions: BaseProviderOptions
    {
        public SQLiteOptions()
        {
            this.CachingProviderType = CachingProviderType.SQLite;
        }

        public SQLiteDBOptions DBConfig { get; set; } = new SQLiteDBOptions();
    }
}
