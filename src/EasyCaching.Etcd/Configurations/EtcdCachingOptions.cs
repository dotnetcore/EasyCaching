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
        /// Etcd access UserName
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Etcd access Pwd
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Etcd timeout with Milliseconds
        /// </summary>
        public long Timeout { get; set; } = 3000;
    }
}