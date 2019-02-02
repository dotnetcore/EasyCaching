namespace EasyCaching.HybridCache
{
    using System;

    public class NotFoundCachingProviderException : Exception
    {
        public NotFoundCachingProviderException(string message) : base(message)
        { }
    }
}
