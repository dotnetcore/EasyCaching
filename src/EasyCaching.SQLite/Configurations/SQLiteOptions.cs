namespace EasyCaching.SQLite
{
    using EasyCaching.Core;
    using EasyCaching.Core.Configurations;

    public class SQLiteOptions: BaseProviderOptions
    {
        public SQLiteOptions()
        {

        }

        public SQLiteDBOptions DBConfig { get; set; } = new SQLiteDBOptions();
    }
}
