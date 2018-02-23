namespace EasyCaching.Serialization.Protobuf
{    
    using ProtoBuf.Meta;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    // borrowed from https://github.com/fnicollier/AutoProtobuf/blob/master/src/AutoProtobuf/SerializerBuilder.cs

    public static class SerializerBuilder
    {
        private const BindingFlags Flags = BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        private static readonly Dictionary<Type, HashSet<Type>> SubTypes = new Dictionary<Type, HashSet<Type>>();
        private static readonly ConcurrentBag<Type> BuiltTypes = new ConcurrentBag<Type>();
        private static readonly Type ObjectType = typeof(object);

        /// <summary>
        /// Build the ProtoBuf serializer from the generic <see cref="Type">type</see>.
        /// </summary>
        /// <typeparam name="T">The type of build the serializer for.</typeparam>
        public static void Build<T>()
        {
            var type = typeof(T);
            Build(type);
        }

        /// <summary>
        /// Build the ProtoBuf serializer from the data's <see cref="Type">type</see>.
        /// </summary>
        /// <typeparam name="T">The type of build the serializer for.</typeparam>
        /// <param name="data">The data who's type a serializer will be made.</param>
        // ReSharper disable once UnusedParameter.Global
        public static void Build<T>(T data)
        {
            Build<T>();
        }

        /// <summary>
        /// Build the ProtoBuf serializer for the <see cref="Type">type</see>.
        /// </summary>
        /// <param name="type">The type of build the serializer for.</param>
        public static void Build(Type type)
        {
            if (BuiltTypes.Contains(type))
            {
                return;
            }

            lock (type)
            {
                if (RuntimeTypeModel.Default.CanSerialize(type))
                {
                    if (type.IsGenericType)
                    {
                        BuildGenerics(type);
                    }

                    return;
                }

                var meta = RuntimeTypeModel.Default.Add(type, false);
                var fields = GetFields(type);

                meta.Add(fields.Select(m => m.Name).ToArray());
                meta.UseConstructor = false;

                BuildBaseClasses(type);
                BuildGenerics(type);

                foreach (var memberType in fields.Select(f => f.FieldType).Where(t => !t.IsPrimitive))
                {
                    Build(memberType);
                }

                BuiltTypes.Add(type);
            }
        }

        /// <summary>
        /// Gets the fields for a type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        private static FieldInfo[] GetFields(Type type)
        {
            return type.GetFields(Flags);
        }

        /// <summary>
        /// Builds the base class serializers for a type.
        /// </summary>
        /// <param name="type">The type.</param>
        private static void BuildBaseClasses(Type type)
        {
            var baseType = type.BaseType;
            var inheritingType = type;


            while (baseType != null && baseType != ObjectType)
            {
                HashSet<Type> baseTypeEntry;

                if (!SubTypes.TryGetValue(baseType, out baseTypeEntry))
                {
                    baseTypeEntry = new HashSet<Type>();
                    SubTypes.Add(baseType, baseTypeEntry);
                }

                if (!baseTypeEntry.Contains(inheritingType))
                {
                    Build(baseType);
                    RuntimeTypeModel.Default[baseType].AddSubType(baseTypeEntry.Count + 500, inheritingType);
                    baseTypeEntry.Add(inheritingType);
                }

                inheritingType = baseType;
                baseType = baseType.BaseType;
            }
        }

        /// <summary>
        /// Builds the serializers for the generic parameters for a given type.
        /// </summary>
        /// <param name="type">The type.</param>
        private static void BuildGenerics(Type type)
        {
            if (type.IsGenericType || (type.BaseType != null && type.BaseType.IsGenericType))
            {
                var generics = type.IsGenericType ? type.GetGenericArguments() : type.BaseType.GetGenericArguments();

                foreach (var generic in generics)
                {
                    Build(generic);
                }
            }
        }
    }
}
