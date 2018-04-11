namespace EasyCaching.SQLite
{
    using EasyCaching.Core.Internal;

    public class SQLiteOptions: BaseProviderOptions
    {
        public SQLiteOptions()
        {
            this.CachingProviderType = CachingProviderType.SQLite;
        }
    }
}
