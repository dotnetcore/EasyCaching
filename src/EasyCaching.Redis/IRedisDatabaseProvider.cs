namespace EasyCaching.Redis
{
    using StackExchange.Redis;

    /// <summary>
    /// Redis database provider.
    /// </summary>
    public interface IRedisDatabaseProvider
    {
        /// <summary>
        /// Gets the database.
        /// </summary>
        /// <returns>The database.</returns>
        IDatabase GetDatabase();
    }
}
