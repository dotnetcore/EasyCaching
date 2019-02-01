namespace EasyCaching.Bus.Redis
{
    using EasyCaching.Core.Internal;
    
    public class RedisBusOptions : BaseRedisOptions
    {
        public int Database { get; set; } = 0;
    }
}
