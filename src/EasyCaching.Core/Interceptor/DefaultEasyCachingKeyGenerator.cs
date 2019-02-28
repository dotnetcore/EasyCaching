namespace EasyCaching.Core.Interceptor
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    /// <summary>
    /// Default easycaching key generator.
    /// </summary>
    public class DefaultEasyCachingKeyGenerator : IEasyCachingKeyGenerator
    {
        private const char LinkChar = ':';

        public string GetCacheKey(MethodInfo methodInfo, object[] args, string prefix)
        {
            var methodArguments = args?.Any() == true
                                      ? args.Select(ParameterCacheKeys.GenerateCacheKey)
                                      : new[] { "0" };
            return GenerateCacheKey(methodInfo, prefix, methodArguments);
        }

        public string GetCacheKeyPrefix(MethodInfo methodInfo, string prefix)
        {
            if (!string.IsNullOrWhiteSpace(prefix)) return $"{prefix}{LinkChar}";

            var typeName = methodInfo.DeclaringType?.Name;
            var methodName = methodInfo.Name;

            return $"{typeName}{LinkChar}{methodName}{LinkChar}";
        }

        private string GenerateCacheKey(MethodInfo methodInfo, string prefix, IEnumerable<string> parameters)
        {
            var cacheKeyPrefix = GetCacheKeyPrefix(methodInfo, prefix);

            var builder = new StringBuilder();
            builder.Append(cacheKeyPrefix);
            builder.Append(string.Join(LinkChar.ToString(), parameters));
            return builder.ToString();
        }
    }
}
