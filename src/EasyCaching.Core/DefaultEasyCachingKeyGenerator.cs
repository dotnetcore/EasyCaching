namespace EasyCaching.Core
{
    using EasyCaching.Core.Internal;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Security.Cryptography;
    using System.Text;

    /// <summary>
    /// Default easycaching key generator.
    /// </summary>
    public class DefaultEasyCachingKeyGenerator : IEasyCachingKeyGenerator
    {
        /// <summary>
        /// The link char of cache key.
        /// </summary>
        private char _linkChar = ':';

        /// <summary>
        /// Gets the cache key.
        /// </summary>
        /// <returns>The cache key.</returns>
        /// <param name="methodInfo">Method info.</param>
        /// <param name="prefix">Prefix.</param>
        public string GetCacheKey(MethodInfo methodInfo, string prefix)
        {
            if(string.IsNullOrWhiteSpace(prefix))
            {
                var typeName = methodInfo.DeclaringType.Name;
                var methodName = methodInfo.Name;

                var methodArguments = this.FormatArgumentsToPartOfCacheKey(methodInfo.GetParameters());

                return this.GenerateCacheKey(typeName, methodName, methodArguments);
            }
            else
            {
                var methodArguments = this.FormatArgumentsToPartOfCacheKey(methodInfo.GetParameters());

                return this.GenerateCacheKey(string.Empty, prefix, methodArguments);
            }
        }

        /// <summary>
        /// Formats the arguments to part of cache key.
        /// </summary>
        /// <returns>The arguments to part of cache key.</returns>
        /// <param name="methodArguments">Method arguments.</param>
        private IList<string> FormatArgumentsToPartOfCacheKey(IList<ParameterInfo> methodArguments)
        {
            return methodArguments.Select(this.GetArgumentValue).ToList();
        }

        /// <summary>
        /// Generates the cache key.
        /// </summary>
        /// <returns>The cache key.</returns>
        /// <param name="typeName">Type name.</param>
        /// <param name="methodName">Method name.</param>
        /// <param name="parameters">Parameters.</param>
        private string GenerateCacheKey(string typeName, string methodName, IList<string> parameters)
        {
            var builder = new StringBuilder();

            builder.Append(typeName);
            builder.Append(_linkChar);

            builder.Append(methodName);
            builder.Append(_linkChar);

            foreach (var param in parameters)
            {
                builder.Append(param);
                builder.Append(_linkChar);
            }

            var str = builder.ToString().TrimEnd(_linkChar);

            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] data = sha1.ComputeHash(Encoding.UTF8.GetBytes(str));
                return Convert.ToBase64String(data, Base64FormattingOptions.None);
            }
        }

        /// <summary>
        /// Gets the argument value.
        /// </summary>
        /// <returns>The argument value.</returns>
        /// <param name="arg">Argument.</param>
        private string GetArgumentValue(object arg)
        {
            if (arg is int || arg is long || arg is string)
                return arg.ToString();

            if (arg is DateTime)
                return ((DateTime)arg).ToString("yyyyMMddHHmmss");

            if (arg is ICachable)
                return ((ICachable)arg).CacheKey;

            return null;
        }
    }
}
