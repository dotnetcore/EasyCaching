namespace EasyCaching.Core
{
    /// <summary>
    /// EasyCaching const value.
    /// </summary>
    public class EasyCachingConstValue
    {
        /// <summary>
        /// The config section.
        /// </summary>
        public const string ConfigSection = "easycaching";

        /// <summary>
        /// The redis section.
        /// </summary>
        public const string RedisSection = "easycaching:redis";

        /// <summary>
        /// The CSRedis section.
        /// </summary>
        public const string CSRedisSection = "easycaching:csredis";

        /// <summary>
        /// The memcached section.
        /// </summary>
        public const string MemcachedSection = "easycaching:memcached";

        /// <summary>
        /// The SQLite section.
        /// </summary>
        public const string SQLiteSection = "easycaching:sqlite";

        /// <summary>
        /// The in-memory section.
        /// </summary>
        public const string InMemorySection = "easycaching:inmemory";

        /// <summary>
        /// The disk section.
        /// </summary>
        public const string DiskSection = "easycaching:disk";

        /// <summary>
        /// The hybrid section.
        /// </summary>
        public const string HybridSection = "easycaching:hybrid";

        /// <summary>
        /// The redis bus section.
        /// </summary>
        public const string RedisBusSection = "easycaching:redisbus";

        /// <summary>
        /// The rabbitMQ Bus section.
        /// </summary>
        public const string RabbitMQBusSection = "easycaching:rabbitmqbus";

        /// <summary>
        /// The kafka bus section.
        /// </summary>
        public const string KafkaBusSection = "easycaching:kafkabus";

        /// <summary>
        /// The zookeeper bus section.
        /// </summary>
        public const string ZookeeperBusSection = "easycaching:zookeeperbus";


        /// <summary>
        /// The default name of the in-memory.
        /// </summary>
        public const string DefaultInMemoryName = "DefaultInMemory";

        /// <summary>
        /// The default name of the redis.
        /// </summary>
        public const string DefaultRedisName = "DefaultRedis";

        /// <summary>
        /// The default name of the CSRedis.
        /// </summary>
        public const string DefaultCSRedisName = "DefaultCSRedis";

        /// <summary>
        /// The default name of the memcached.
        /// </summary>
        public const string DefaultMemcachedName = "DefaultMemcached";

        /// <summary>
        /// The default name of the SQLite.
        /// </summary>
        public const string DefaultSQLiteName = "DefaultSQLite";

        /// <summary>
        /// The default name of the disk.
        /// </summary>
        public const string DefaultDiskName = "DefaultDisk";

        /// <summary>
        /// The default name of the hybrid.
        /// </summary>
        public const string DefaultHybridName = "DefaultHybrid";

        /// <summary>
        /// The default name of the serializer.
        /// </summary>
        public const string DefaultSerializerName = "binary";

        /// <summary>
        /// The default name of the LiteDB.
        /// </summary>
        public const string DefaultLiteDBName = "DefaultLiteDB";
        /// <summary>
        /// The LiteDB Bus section.
        /// </summary>
        public const string LiteDBSection= "easycaching:litedb";

        /// <summary>
        /// The default name of the FasterKv
        /// </summary>
        public const string DefaultFasterKvName = "DefaultFasterKvName";
        
        /// <summary>
        /// The FasterKv section.
        /// </summary>
        public const string FasterKvSection= "easycaching:fasterKv";

        public const string NotFoundCliExceptionMessage = "Can not find the matched client instance, client name is {0}";

        public const string NotFoundSerExceptionMessage = "Can not find the matched serializer instance, serializer name is {0}";
    }
}
