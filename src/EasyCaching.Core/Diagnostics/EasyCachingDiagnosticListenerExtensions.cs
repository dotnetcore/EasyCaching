namespace EasyCaching.Core.Diagnostics
{
    using System.Diagnostics;

    internal static class EasyCachingDiagnosticListenerExtensions
    {
        /// <summary>
        /// The name of the diagnostic listener.
        /// </summary>
        public const string DiagnosticListenerName = "EasyCachingDiagnosticListener";

        public const string EasyCachingSetCache = nameof(WriteSetCache);
        public const string EasyCachingExistsCache = nameof(WriteExistsCache);
        public const string EasyCachingFlushCache = nameof(WriteFlushCache);
        public const string EasyCachingRemoveCache = nameof(WriteRemoveCache);
        public const string EasyCachingGetCache = nameof(WriteGetCache);
        public const string EasyCachingGetCount = nameof(WriteGetCount);
        public const string EasyCachingSetAll = nameof(WriteSetAll);
        public const string EasyCachingPublishMessage = nameof(WritePublishMessage);
        public const string EasyCachingSubscribeMessage = nameof(WriteSubscribeMessage);

        public static void WriteSubscribeMessage(this DiagnosticListener @this, object message)
        {
            if (@this.IsEnabled(EasyCachingSubscribeMessage))
            {
                @this.Write(EasyCachingSubscribeMessage, new
                {
                    Message = message
                });
            }
        }

        public static void WritePublishMessage(this DiagnosticListener @this, string topic, object message)
        {
            if (@this.IsEnabled(EasyCachingPublishMessage))
            {
                @this.Write(EasyCachingPublishMessage, new
                {
                    Topic = topic,
                    Message = message
                });
            }
        }

        public static void WriteSetCache(this DiagnosticListener @this, SetCacheEventData eventData)
        {
            if (@this.IsEnabled(EasyCachingSetCache))
            {
                @this.Write(EasyCachingSetCache, eventData);
            }
        }

        public static void WriteRemoveCache(this DiagnosticListener @this, RemoveCacheEventData eventData)
        {
            if (@this.IsEnabled(EasyCachingRemoveCache))
            {
                @this.Write(EasyCachingRemoveCache, eventData);
            }
        }

        public static void WriteGetCache(this DiagnosticListener @this, GetCacheEventData eventData)
        {
            if (@this.IsEnabled(EasyCachingGetCache))
            {
                @this.Write(EasyCachingGetCache, eventData);
            }
        }

        public static void WriteSetAll(this DiagnosticListener @this, SetAllEventData eventData)
        {
            if (@this.IsEnabled(EasyCachingSetAll))
            {
                @this.Write(EasyCachingSetAll, eventData);
            }
        }

        public static void WriteGetCount(this DiagnosticListener @this, GetCountEventData eventData)
        {
            if (@this.IsEnabled(EasyCachingGetCount))
            {
                @this.Write(EasyCachingGetCount, eventData);
            }
        }

        public static void WriteExistsCache(this DiagnosticListener @this, ExistsCacheEventData eventData)
        {
            if (@this.IsEnabled(EasyCachingExistsCache))
            {
                @this.Write(EasyCachingExistsCache, eventData);
            }
        }

        public static void WriteFlushCache(this DiagnosticListener @this, EventData eventData)
        {
            if (@this.IsEnabled(EasyCachingFlushCache))
            {
                @this.Write(EasyCachingFlushCache, eventData);
            }
        }
    }
}
