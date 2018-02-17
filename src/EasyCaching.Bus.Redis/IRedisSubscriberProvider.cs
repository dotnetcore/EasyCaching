namespace EasyCaching.Sync.Redis
{
    using StackExchange.Redis;

    /// <summary>
    /// Redis database provider.
    /// </summary>
    public interface IRedisSubscriberProvider
    {
        /// <summary>
        /// Gets the subscriber.
        /// </summary>
        /// <returns>The subscriber.</returns>
        ISubscriber GetSubscriber();
    }
}
