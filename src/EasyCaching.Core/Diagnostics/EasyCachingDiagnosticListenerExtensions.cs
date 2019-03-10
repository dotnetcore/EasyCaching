namespace EasyCaching.Core.Diagnostics
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// Extension methods on the DiagnosticListener class to log EasyCaching
    /// </summary>
    internal static class EasyCachingDiagnosticListenerExtensions
    {
        public const string DiagnosticListenerName = "EasyCachingDiagnosticListener";

        private const string EasyCachingPrefix = "EasyCaching.";

        public const string EasyCachingBeforeSetCache = EasyCachingPrefix + nameof(WriteSetCacheBefore);
        public const string EasyCachingAfterSetCache = EasyCachingPrefix + nameof(WriteSetCacheAfter);
        public const string EasyCachingErrorSetCache = EasyCachingPrefix + nameof(WriteSetCacheError);

        public const string EasyCachingBeforeRemoveCache = EasyCachingPrefix + nameof(WriteRemoveCacheBefore);
        public const string EasyCachingAfterRemoveCache = EasyCachingPrefix + nameof(WriteRemoveCacheAfter);
        public const string EasyCachingErrorRemoveCache = EasyCachingPrefix + nameof(WriteRemoveCacheError);

        public const string EasyCachingBeforeGetCache = EasyCachingPrefix + nameof(WriteGetCacheBefore);
        public const string EasyCachingAfterGetCache = EasyCachingPrefix + nameof(WriteGetCacheAfter);
        public const string EasyCachingErrorGetCache = EasyCachingPrefix + nameof(WriteGetCacheError);

        public const string EasyCachingBeforeExistsCache = EasyCachingPrefix + nameof(WriteExistsCacheBefore);
        public const string EasyCachingAfterExistsCache = EasyCachingPrefix + nameof(WriteExistsCacheAfter);
        public const string EasyCachingErrorExistsCache = EasyCachingPrefix + nameof(WriteExistsCacheError);

        public const string EasyCachingBeforeFlushCache = EasyCachingPrefix + nameof(WriteFlushCacheBefore);
        public const string EasyCachingAfterFlushCache = EasyCachingPrefix + nameof(WriteFlushCacheAfter);
        public const string EasyCachingErrorFlushCache = EasyCachingPrefix + nameof(WriteFlushCacheError);

        public const string EasyCachingBeforePublishMessage = EasyCachingPrefix + nameof(WritePublishMessageBefore);
        public const string EasyCachingAfterPublishMessage = EasyCachingPrefix + nameof(WritePublishMessageAfter);
        public const string EasyCachingErrorPublishMessage = EasyCachingPrefix + nameof(WritePublishMessageError);

        public const string EasyCachingBeforeSubscribeMessage = EasyCachingPrefix + nameof(WriteSubscribeMessageBefore);
        public const string EasyCachingAfterSubscribeMessage = EasyCachingPrefix + nameof(WriteSubscribeMessageAfter);
        public const string EasyCachingErrorSubscribeMessage = EasyCachingPrefix + nameof(WriteSubscribeMessageError);

        public static void WriteSetCacheError(this DiagnosticListener @this, Guid operationId, Exception ex)
        {
            if (@this.IsEnabled(EasyCachingErrorSetCache))
            {
                @this.Write(EasyCachingErrorSetCache, new
                {
                    OperationId = operationId,
                    Exception = ex,
                    Timestamp = Stopwatch.GetTimestamp()
                });
            }
        }

        public static void WriteRemoveCacheError(this DiagnosticListener @this, Guid operationId, Exception ex)
        {
            if (@this.IsEnabled(EasyCachingErrorRemoveCache))
            {
                @this.Write(EasyCachingErrorRemoveCache, new
                {
                    OperationId = operationId,
                    Exception = ex,
                    Timestamp = Stopwatch.GetTimestamp()
                });
            }
        }

        public static void WriteGetCacheError(this DiagnosticListener @this, Guid operationId, Exception ex)
        {
            if (@this.IsEnabled(EasyCachingErrorGetCache))
            {
                @this.Write(EasyCachingErrorGetCache, new
                {
                    OperationId = operationId,
                    Exception = ex,
                    Timestamp = Stopwatch.GetTimestamp()
                });
            }
        }

        public static void WriteExistsCacheError(this DiagnosticListener @this, Guid operationId, Exception ex)
        {
            if (@this.IsEnabled(EasyCachingErrorExistsCache))
            {
                @this.Write(EasyCachingErrorExistsCache, new
                {
                    OperationId = operationId,
                    Exception = ex,
                    Timestamp = Stopwatch.GetTimestamp()
                });
            }
        }

        public static void WriteFlushCacheError(this DiagnosticListener @this, Guid operationId, Exception ex)
        {
            if (@this.IsEnabled(EasyCachingErrorFlushCache))
            {
                @this.Write(EasyCachingErrorFlushCache, new
                {
                    OperationId = operationId,
                    Exception = ex,
                    Timestamp = Stopwatch.GetTimestamp()
                });
            }
        }

        public static void WritePublishMessageError(this DiagnosticListener @this, Guid operationId, Exception ex)
        {
            if (@this.IsEnabled(EasyCachingErrorPublishMessage))
            {
                @this.Write(EasyCachingErrorPublishMessage, new
                {
                    OperationId = operationId,
                    Exception = ex,
                    Timestamp = Stopwatch.GetTimestamp()
                });
            }
        }

        public static void WriteSubscribeMessageError(this DiagnosticListener @this, Guid operationId, Exception ex)
        {
            if (@this.IsEnabled(EasyCachingErrorSubscribeMessage))
            {
                @this.Write(EasyCachingErrorSubscribeMessage, new
                {
                    OperationId = operationId,
                    Exception = ex,
                    Timestamp = Stopwatch.GetTimestamp()
                });
            }
        }

        public static void WriteSetCacheAfter(this DiagnosticListener @this, Guid operationId)
        {
            if (@this.IsEnabled(EasyCachingAfterSetCache))
            {
                @this.Write(EasyCachingAfterSetCache, new
                {
                    OperationId = operationId,
                    Timestamp = Stopwatch.GetTimestamp()
                });
            }
        }

        public static void WriteRemoveCacheAfter(this DiagnosticListener @this, Guid operationId)
        {
            if (@this.IsEnabled(EasyCachingAfterRemoveCache))
            {
                @this.Write(EasyCachingAfterRemoveCache, new
                {
                    OperationId = operationId,
                    Timestamp = Stopwatch.GetTimestamp()
                });
            }
        }

        public static void WriteGetCacheAfter(this DiagnosticListener @this, Guid operationId)
        {
            if (@this.IsEnabled(EasyCachingAfterGetCache))
            {
                @this.Write(EasyCachingAfterGetCache, new
                {
                    OperationId = operationId,
                    Timestamp = Stopwatch.GetTimestamp()
                });
            }
        }

        public static void WriteExistsCacheAfter(this DiagnosticListener @this, Guid operationId)
        {
            if (@this.IsEnabled(EasyCachingAfterExistsCache))
            {
                @this.Write(EasyCachingAfterExistsCache, new
                {
                    OperationId = operationId,
                    Timestamp = Stopwatch.GetTimestamp()
                });
            }
        }

        public static void WriteFlushCacheAfter(this DiagnosticListener @this, Guid operationId)
        {
            if (@this.IsEnabled(EasyCachingAfterFlushCache))
            {
                @this.Write(EasyCachingAfterFlushCache, new
                {
                    OperationId = operationId,
                    Timestamp = Stopwatch.GetTimestamp()
                });
            }
        }

        public static void WritePublishMessageAfter(this DiagnosticListener @this, Guid operationId)
        {
            if (@this.IsEnabled(EasyCachingAfterPublishMessage))
            {
                @this.Write(EasyCachingAfterPublishMessage, new
                {
                    OperationId = operationId,
                    Timestamp = Stopwatch.GetTimestamp()
                });
            }
        }

        public static void WriteSubscribeMessageAfter(this DiagnosticListener @this, Guid operationId)
        {
            if (@this.IsEnabled(EasyCachingAfterSubscribeMessage))
            {
                @this.Write(EasyCachingAfterSubscribeMessage, new
                {
                    OperationId = operationId,
                    Timestamp = Stopwatch.GetTimestamp()
                });
            }
        }

        public static Guid WriteSetCacheBefore(this DiagnosticListener @this, BeforeSetRequestEventData eventData)
        {
            if (@this.IsEnabled(EasyCachingBeforeSetCache))
            {
                Guid operationId = Guid.NewGuid();

                @this.Write(EasyCachingBeforeSetCache, new
                {
                    OperationId = operationId,
                    EventData = eventData,
                    Timestamp = Stopwatch.GetTimestamp()
                });

                return operationId;
            }

            return Guid.Empty;
        }

        public static Guid WriteRemoveCacheBefore(this DiagnosticListener @this, BeforeRemoveRequestEventData eventData)
        {
            if (@this.IsEnabled(EasyCachingBeforeRemoveCache))
            {
                Guid operationId = Guid.NewGuid();
                @this.Write(EasyCachingBeforeRemoveCache, new
                {
                    OperationId = operationId,
                    EventData = eventData,
                    Timestamp = Stopwatch.GetTimestamp()
                });
                return operationId;
            }

            return Guid.Empty;
        }

        public static Guid WriteGetCacheBefore(this DiagnosticListener @this, BeforeGetRequestEventData eventData)
        {
            if (@this.IsEnabled(EasyCachingBeforeGetCache))
            {
                Guid operationId = Guid.NewGuid();

                @this.Write(EasyCachingBeforeGetCache, new
                {
                    OperationId = operationId,
                    EventData = eventData,
                    Timestamp = Stopwatch.GetTimestamp()
                });

                return operationId;
            }

            return Guid.Empty;
        }

        public static Guid WriteExistsCacheBefore(this DiagnosticListener @this, BeforeExistsRequestEventData eventData)
        {
            if (@this.IsEnabled(EasyCachingBeforeExistsCache))
            {
                Guid operationId = Guid.NewGuid();

                @this.Write(EasyCachingBeforeExistsCache, new
                {
                    OperationId = operationId,
                    EventData = eventData,
                    Timestamp = Stopwatch.GetTimestamp()
                });

                return operationId;
            }

            return Guid.Empty;
        }

        public static Guid WriteFlushCacheBefore(this DiagnosticListener @this, EventData eventData)
        {
            if (@this.IsEnabled(EasyCachingBeforeFlushCache))
            {
                Guid operationId = Guid.NewGuid();

                @this.Write(EasyCachingBeforeFlushCache, new
                {
                    OperationId = operationId,
                    EventData = eventData,
                    Timestamp = Stopwatch.GetTimestamp()
                });

                return operationId;
            }

            return Guid.Empty;
        }

        public static Guid WriteSubscribeMessageBefore(this DiagnosticListener @this, BeforeSubscribeMessageRequestEventData eventData)
        {
            if (@this.IsEnabled(EasyCachingBeforeSubscribeMessage))
            {
                Guid operationId = Guid.NewGuid();

                @this.Write(EasyCachingBeforeSubscribeMessage, new
                {
                    OperationId = operationId,
                    EventData = eventData,
                    Timestamp = Stopwatch.GetTimestamp()
                });

                return operationId;
            }

            return Guid.Empty;
        }

        public static Guid WritePublishMessageBefore(this DiagnosticListener @this, BeforePublishMessageRequestEventData eventData)
        {
            if (@this.IsEnabled(EasyCachingBeforePublishMessage))
            {
                Guid operationId = Guid.NewGuid();

                @this.Write(EasyCachingBeforePublishMessage, new
                {
                    OperationId = operationId,
                    EventData = eventData,
                    Timestamp = Stopwatch.GetTimestamp()
                });

                return operationId;
            }

            return Guid.Empty;
        }
    }
}
