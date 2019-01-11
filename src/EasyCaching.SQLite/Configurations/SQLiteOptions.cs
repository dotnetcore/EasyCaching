namespace EasyCaching.SQLite
{
    using EasyCaching.Core;
    using EasyCaching.Core.Internal;

    public class SQLiteOptions: BaseProviderOptions
    {
        public SQLiteOptions()
        {
            this.CachingProviderType = CachingProviderType.SQLite;
        }

        public SQLiteDBOptions DBConfig { get; set; } = new SQLiteDBOptions();
    }
}
