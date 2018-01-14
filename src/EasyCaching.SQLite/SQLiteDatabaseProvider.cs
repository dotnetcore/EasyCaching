namespace EasyCaching.SQLite
{
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
        private readonly SQLiteCacheOption _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.SQLite.SQLiteDatabaseProvider"/> class.
        /// </summary>
        /// <param name="optionAction">Option action.</param>
        public SQLiteDatabaseProvider(IOptions<SQLiteCacheOption> optionAction)
        {
            this._options = optionAction.Value;
        }

        /// <summary>
        /// The conn.
        /// </summary>
        private static SqliteConnection _conn;

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
