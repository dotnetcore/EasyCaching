namespace EasyCaching.CSRedis
{
    using global::CSRedis;

    public interface IRedisDatabaseProvider
    {
        /// <summary>
        /// Gets the client.
        /// </summary>
        /// <returns>The client.</returns>
        CSRedisClient GetClient();

        /// <summary>
        /// Gets the name of the DBP rovider.
        /// </summary>
        /// <value>The name of the DBP rovider.</value>
        string DBProviderName { get; }
    }  
}