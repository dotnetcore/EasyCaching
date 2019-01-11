namespace EasyCaching.SQLite
{
    using Microsoft.Data.Sqlite;

    /// <summary>
    /// SQLite database provider.
    /// </summary>
    public interface ISQLiteDatabaseProvider
    {
        /// <summary>
        /// Gets the connection.
        /// </summary>
        /// <returns>The connection.</returns>
        SqliteConnection GetConnection();

        string DBProviderName { get; }
    }
}
