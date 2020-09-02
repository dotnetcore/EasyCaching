namespace EasyCaching.LiteDB
{
    using global::LiteDB;

    /// <summary>
    /// LiteDB database provider.
    /// </summary>
    public interface ILiteDBDatabaseProvider
    {
        /// <summary>
        /// Gets the connection.
        /// </summary>
        /// <returns>The connection.</returns>
        LiteDatabase GetConnection();

        string DBProviderName { get; }
    }
}
