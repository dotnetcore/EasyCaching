using Castle.DynamicProxy;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EasyCaching.Interceptor.Castle
{
    public static class ReflectionExtensions
    {
        public static bool IsReturnTask(this MethodInfo methodInfo)
        {
            if (methodInfo == null)
            {
                throw new ArgumentNullException(nameof(methodInfo));
            }
            var returnType = methodInfo.ReturnType.GetTypeInfo();
            return returnType.IsTaskWithResult();
        }

        public static Task<object> UnwrapAsyncReturnValue(this IInvocation invocation)
        {
            if (invocation == null)
            {
                throw new ArgumentNullException(nameof(invocation));
            }

            var serviceMethod = invocation.Method ?? invocation.MethodInvocationTarget;

            if (!serviceMethod.IsReturnTask())
            {
                throw new InvalidOperationException("This operation only support asynchronous method.");
            }

            var returnValue = invocation.ReturnValue;
            if (returnValue == null)
            {
                return null;
            }

            var returnTypeInfo = returnValue.GetType().GetTypeInfo();
            return Unwrap(returnValue, returnTypeInfo);
        }

        private static async Task<object> Unwrap(object value, TypeInfo valueTypeInfo)
        {
            object result = null;

            if (valueTypeInfo.IsTaskWithResult())
            {
                // Is there better solution to unwrap ?
                result = (object) (await (dynamic) value);
            }
            else if (value is Task)
            {
                return null;
            }
            else
            {
                result = value;
            }

            if (result == null)
            {
                return null;
            }

            var resultTypeInfo = result.GetType().GetTypeInfo();
            if (IsAsyncType(resultTypeInfo))
            {
                return Unwrap(result, resultTypeInfo);
            }

            return result;
        }

        private static bool IsAsyncType(TypeInfo typeInfo)
        {
            if (typeInfo.IsTask())
            {
                return true;
            }

            if (typeInfo.IsTaskWithResult())
            {
                return true;
            }
            

            return false;
        }
    }
}
