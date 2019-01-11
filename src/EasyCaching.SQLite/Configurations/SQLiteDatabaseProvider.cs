namespace EasyCaching.SQLite
{
    using EasyCaching.Core;
    using Microsoft.Data.Sqlite;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// SQLite database provider.
    /// </summary>
    public class SQLiteDatabaseProvider : ISQLiteDatabaseProvider
    {
        /// <summary>
        /// The options.
        /// </summary>
        private readonly SQLiteDBOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.SQLite.SQLiteDatabaseProvider"/> class.
        /// </summary>
        /// <param name="optionAction">Option action.</param>
        public SQLiteDatabaseProvider(IOptionsMonitor<SQLiteOptions> optionAction)
        {
            this._options = optionAction.CurrentValue.DBConfig;
        }


        public SQLiteDatabaseProvider(string name , SQLiteOptions options)
        {
            this._name = name;
            this._options = options.DBConfig;
        }

        /// <summary>
        /// The conn.
        /// </summary>
        private static SqliteConnection _conn;

        private readonly string _name = EasyCachingConstValue.DefaultSQLiteName;
        public string DBProviderName => _name;

        /// <summary>
        /// Gets the connection.
        /// </summary>
        /// <returns>The connection.</returns>
        public SqliteConnection GetConnection()
        {
            if(_conn == null)
            {
                SqliteConnectionStringBuilder builder = new SqliteConnectionStringBuilder()
                {
                    DataSource = _options.DataSource,
                    Mode = _options.OpenMode,
                    Cache = _options.CacheMode
                };

                _conn = new SqliteConnection(builder.ToString());

            }
            return _conn;
        }
    }
}
