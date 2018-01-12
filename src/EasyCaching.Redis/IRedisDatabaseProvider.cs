namespace EasyCaching.Redis
{
    using StackExchange.Redis;

    public interface IRedisDatabaseProvider
    {
        IDatabase GetDatabase();
    }
}
