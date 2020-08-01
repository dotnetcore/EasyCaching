using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading.Tasks;

namespace EasyCaching.Interceptor.Castle
{
    public static class TypeExtensions
    {
        private static readonly ConcurrentDictionary<TypeInfo, bool> isTaskOfTCache = new ConcurrentDictionary<TypeInfo, bool>();

        public static bool IsTaskWithResult(this TypeInfo typeInfo)
        {
            if (typeInfo == null)
            {
                throw new ArgumentNullException(nameof(typeInfo));
            }
            return isTaskOfTCache.GetOrAdd(typeInfo, Info => Info.IsGenericType && typeof(Task).GetTypeInfo().IsAssignableFrom(Info));
        }

        public static bool IsTask(this TypeInfo typeInfo)
        {
            if (typeInfo == null)
            {
                throw new ArgumentNullException(nameof(typeInfo));
            }
            return typeInfo.AsType() == typeof(Task);
        }
        
    }
}
