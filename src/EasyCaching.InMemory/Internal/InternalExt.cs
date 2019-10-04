namespace EasyCaching.InMemory
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public static class ObjectExtensions
    {
        public static DateTimeOffset SafeAdd(this DateTimeOffset date, TimeSpan value)
        {
            if (date.Ticks + value.Ticks < DateTimeOffset.MinValue.Ticks)
                return DateTimeOffset.MinValue;

            if (date.Ticks + value.Ticks > DateTimeOffset.MaxValue.Ticks)
                return DateTimeOffset.MaxValue;

            return date.Add(value);
        }
    }

    internal static class TypeHelper
    {
        public static readonly Type ObjectType = typeof(object);
        public static readonly Type StringType = typeof(string);
        public static readonly Type CharType = typeof(char);
        public static readonly Type NullableCharType = typeof(char?);
        public static readonly Type DateTimeType = typeof(DateTime);
        public static readonly Type NullableDateTimeType = typeof(DateTime?);
        public static readonly Type BoolType = typeof(bool);
        public static readonly Type NullableBoolType = typeof(bool?);
        public static readonly Type ByteArrayType = typeof(byte[]);
        public static readonly Type ByteType = typeof(byte);
        public static readonly Type SByteType = typeof(sbyte);
        public static readonly Type SingleType = typeof(float);
        public static readonly Type DecimalType = typeof(decimal);
        public static readonly Type Int16Type = typeof(short);
        public static readonly Type UInt16Type = typeof(ushort);
        public static readonly Type Int32Type = typeof(int);
        public static readonly Type UInt32Type = typeof(uint);
        public static readonly Type Int64Type = typeof(long);
        public static readonly Type UInt64Type = typeof(ulong);
        public static readonly Type DoubleType = typeof(double);

        public static Type ResolveType(string fullTypeName, Type expectedBase = null)
        {
            if (String.IsNullOrEmpty(fullTypeName))
                return null;

            var type = Type.GetType(fullTypeName);
            if (type == null)
            {
                return null;
            }

            if (expectedBase != null && !expectedBase.IsAssignableFrom(type))
            {
                return null;
            }

            return type;
        }

        private static readonly Dictionary<Type, string> _builtInTypeNames = new Dictionary<Type, string> {
            { StringType, "string" },
            { BoolType, "bool" },
            { ByteType, "byte" },
            { SByteType, "sbyte" },
            { CharType, "char" },
            { DecimalType, "decimal" },
            { DoubleType, "double" },
            { SingleType, "float" },
            { Int16Type, "short" },
            { Int32Type, "int" },
            { Int64Type, "long" },
            { ObjectType, "object" },
            { UInt16Type, "ushort" },
            { UInt32Type, "uint" },
            { UInt64Type, "ulong" }
        };
    }

    internal static class TypeExtensions
    {
        public static bool IsNumeric(this Type type)
        {
            if (type.IsArray)
                return false;

            if (type == TypeHelper.ByteType ||
                type == TypeHelper.DecimalType ||
                type == TypeHelper.DoubleType ||
                type == TypeHelper.Int16Type ||
                type == TypeHelper.Int32Type ||
                type == TypeHelper.Int64Type ||
                type == TypeHelper.SByteType ||
                type == TypeHelper.SingleType ||
                type == TypeHelper.UInt16Type ||
                type == TypeHelper.UInt32Type ||
                type == TypeHelper.UInt64Type)
                return true;

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.Single:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return true;
            }

            return false;
        }

        public static bool IsNullableNumeric(this Type type)
        {
            if (type.IsArray)
                return false;

            var t = Nullable.GetUnderlyingType(type);
            return t != null && t.IsNumeric();
        }
    }

    internal static class ReflectionHelper
    {
        public static MethodInfo GetPrivateMethod(this Type t, string methodName)
        {
            return t.GetTypeInfo().GetDeclaredMethod(methodName);
        }

        public static MethodInfo GetPrivateStaticMethod(this Type t, string methodName)
        {
            return t.GetTypeInfo().GetDeclaredMethod(methodName);
        }

        public static bool IsSubclassOfTypeByName(this Type t, string typeName)
        {
            while (t != null)
            {
                if (t.Name == typeName)
                    return true;
                t = t.GetTypeInfo().BaseType;
            }

            return false;
        }
    }
}
