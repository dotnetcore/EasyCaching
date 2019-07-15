namespace EasyCaching.Core.Interceptor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public static class ParameterCacheKeys
    {
        public static string GenerateCacheKey(object parameter)
        {
            if (parameter == null) return string.Empty;
            if (parameter is ICachable cachable) return cachable.CacheKey;
            if (parameter is string key) return key;
            if (parameter is DateTime dateTime) return dateTime.ToString("O");
            if (parameter is DateTimeOffset dateTimeOffset) return dateTimeOffset.ToString("O");
            if (parameter is IEnumerable enumerable) return GenerateCacheKey(enumerable.Cast<object>());
            return parameter.ToString();
        }

        private static string GenerateCacheKey(IEnumerable<object> parameter)
        {
            if (parameter == null) return string.Empty;
            return "[" + string.Join(",", parameter) + "]";
        }
    }
}