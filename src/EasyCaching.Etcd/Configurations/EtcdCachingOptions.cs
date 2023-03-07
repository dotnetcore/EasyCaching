using EasyCaching.Core.Configurations;

namespace EasyCaching.Etcd
{
    /// <summary>
    /// EasyCaching options extensions of Etcd.
    /// </summary>
    public class EtcdCachingOptions : BaseProviderOptions
    {
        /// <summary>
        /// Etcd address
        /// cluster:like "https://localhost:23790,https://localhost:23791,https://localhost:23792"
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 服务名
        /// </summary>
        public string ServerName { get; set; }

        /// <summary>
        /// Etcd access UserName
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Etcd access Pwd
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Etcd timeout
        /// </summary>
        public long Timeout { get; set; }

        /// <summary>
        /// life time ttl
        /// </summary>
        public long TTL { get; set; }
    }
}