namespace EasyCaching.Redis
{
    using StackExchange.Redis;
    using System.Collections.Generic;

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

        /// <summary>
        /// Gets the server list.
        /// </summary>
        /// <returns>The server list.</returns>
        IEnumerable<IServer> GetServerList();

        string DBProviderName { get; }
    }
}
