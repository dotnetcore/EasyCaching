namespace EasyCaching.SQLite
{
    using EasyCaching.Core;
    using Microsoft.Data.Sqlite;
    using System.Collections.Concurrent;
    using System;
    using System.Data;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// SQLite database provider.
    /// </summary>
    public class SQLiteDatabaseProvider : ISQLiteDatabaseProvider
    {
        /// <summary>
        ///     Connections pool
        /// </summary>
        private readonly ConcurrentDictionary<int, SqliteConnection> _conns;

        /// <summary>
        ///     The options.
        /// </summary>
        private readonly SQLiteDBOptions _options;

        /// <summary>
        ///     The builder
        /// </summary>
        private readonly SqliteConnectionStringBuilder _builder;
        
        public SQLiteDatabaseProvider(string name , SQLiteOptions options)
        {
            _name = name;
            _options = options.DBConfig;
            _builder = new SqliteConnectionStringBuilder
            {
                DataSource = _options.DataSource,
                Mode = _options.OpenMode,
                Cache = _options.CacheMode
            };

            _conns = new ConcurrentDictionary<int, SqliteConnection>();
        }

        private readonly string _name = EasyCachingConstValue.DefaultSQLiteName;

        public string DBProviderName => _name;

        /// <summary>
        /// Gets the connection.
        /// </summary>
        /// <returns>The connection.</returns>
        public SqliteConnection GetConnection()
        {
            var threadId = Thread.CurrentThread.ManagedThreadId;
            var con = _conns.GetOrAdd(threadId, CreateNewConnection());

            Task.Run(async () =>
            {
                //keep the connection for 30 minutes
                await Task.Delay(TimeSpan.FromMinutes(30)).ConfigureAwait(false);
                _conns.TryRemove(threadId, out var removingConn);
                if (removingConn?.State == ConnectionState.Closed)
                {
                    removingConn.Dispose();
                }
            });

            return con;
        }

        private SqliteConnection CreateNewConnection()
        {
            return new SqliteConnection(_builder.ToString());
        }
    }
}
