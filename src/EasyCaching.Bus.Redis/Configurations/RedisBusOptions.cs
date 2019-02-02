namespace EasyCaching.Bus.Redis
{
    using EasyCaching.Core.Configurations;

    public class RedisBusOptions : BaseRedisOptions
    {
        public int Database { get; set; } = 0;
    }
}
