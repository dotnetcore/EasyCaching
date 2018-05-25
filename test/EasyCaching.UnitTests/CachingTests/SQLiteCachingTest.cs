namespace EasyCaching.UnitTests
{
    using Dapper;
    using EasyCaching.SQLite;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using System;

    public class SQLiteCachingTest : BaseCachingProviderTest
    {
        private readonly ISQLiteDatabaseProvider _dbProvider;

        public SQLiteCachingTest()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddSQLiteCache(options =>
            {
                options.FileName = "";
                options.FilePath = "";
                options.CacheMode = Microsoft.Data.Sqlite.SqliteCacheMode.Default;
                options.OpenMode = Microsoft.Data.Sqlite.SqliteOpenMode.Memory;
            });

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            _dbProvider = serviceProvider.GetService<ISQLiteDatabaseProvider>();

            var conn = _dbProvider.GetConnection();
            if (conn.State == System.Data.ConnectionState.Closed)
            {
                conn.Open();
            }
            conn.Execute(ConstSQL.CREATESQL);

            _provider = new DefaultSQLiteCachingProvider(_dbProvider, new OptionsWrapper<SQLiteOptions>(new SQLiteOptions()));
            _defaultTs = TimeSpan.FromSeconds(30);
        }
    }
}
