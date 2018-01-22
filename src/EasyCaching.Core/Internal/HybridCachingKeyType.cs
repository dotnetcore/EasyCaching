namespace EasyCaching.Core.Internal
{
    /// <summary>
    /// Autofac key type.
    /// </summary>
    public class HybridCachingKeyType
    {
        /// <summary>
        /// The local key.
        /// </summary>
        public const string LocalKey = "Local";

        /// <summary>
        /// The distributed key.
        /// </summary>
        public const string DistributedKey = "Distributed";
    }
}
