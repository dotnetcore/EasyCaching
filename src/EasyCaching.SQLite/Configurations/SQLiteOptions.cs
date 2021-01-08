namespace EasyCaching.SQLite
{
    using EasyCaching.Core;
    using EasyCaching.Core.Configurations;

    public class SQLiteOptions : BaseProviderOptionsWithDecorator<IEasyCachingProvider>
    {
        public SQLiteDBOptions DBConfig { get; set; } = new SQLiteDBOptions();
    }
}
