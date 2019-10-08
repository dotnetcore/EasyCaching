namespace EasyCaching.InMemory
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    // https://github.com/force-net/DeepCloner/tree/develop/DeepCloner

    internal static class DeepClonerGenerator
    {
        public static T CloneObject<T>(T obj)
        {
            if (obj is ValueType)
            {
                var type = obj.GetType();
                if (typeof(T) == type)
                {
                    if (DeepClonerSafeTypes.CanReturnSameObject(type))
                        return obj;

                    return CloneStructInternal(obj, new DeepCloneState());
                }
            }

            return (T)CloneClassRoot(obj);
        }

        private static object CloneClassRoot(object obj)
        {
            if (obj == null) return null;

            var cloner = (Func<object, DeepCloneState, object>)DeepClonerCache.GetOrAddClass(obj.GetType(), t => GenerateCloner(t, true));

            // null -> should return same type
            if (cloner == null) return obj;

            return cloner(obj, new DeepCloneState());
        }

        internal static object CloneClassInternal(object obj, DeepCloneState state)
        {
            if (obj == null)
                return null;

            var cloner = (Func<object, DeepCloneState, object>)DeepClonerCache.GetOrAddClass(obj.GetType(), t => GenerateCloner(t, true));

            // safe ojbect
            if (cloner == null)
                return obj;

            // loop
            var knownRef = state.GetKnownRef(obj);
            if (knownRef != null)
                return knownRef;

            return cloner(obj, state);
        }

        private static T CloneStructInternal<T>(T obj, DeepCloneState state) // where T : struct
        {
            // no loops, no nulls, no inheritance
            var cloner = GetClonerForValueType<T>();

            // safe ojbect
            if (cloner == null) return obj;

            return cloner(obj, state);
        }

        // when we can't use code generation, we can use these methods
        internal static T[] Clone1DimArraySafeInternal<T>(T[] obj, DeepCloneState state)
        {
            var l = obj.Length;
            var outArray = new T[l];
            state.AddKnownRef(obj, outArray);
            Array.Copy(obj, outArray, obj.Length);
            return outArray;
        }

        internal static T[] Clone1DimArrayStructInternal<T>(T[] obj, DeepCloneState state)
        {
            // not null from called method, but will check it anyway
            if (obj == null) return null;
            var l = obj.Length;
            var outArray = new T[l];
            state.AddKnownRef(obj, outArray);
            var cloner = GetClonerForValueType<T>();
            for (var i = 0; i < l; i++)
                outArray[i] = cloner(obj[i], state);

            return outArray;
        }

        internal static T[] Clone1DimArrayClassInternal<T>(T[] obj, DeepCloneState state)
        {
            // not null from called method, but will check it anyway
            if (obj == null) return null;
            var l = obj.Length;
            var outArray = new T[l];
            state.AddKnownRef(obj, outArray);
            for (var i = 0; i < l; i++)
                outArray[i] = (T)CloneClassInternal(obj[i], state);

            return outArray;
        }

        // relatively frequent case. specially handled
        internal static T[,] Clone2DimArrayInternal<T>(T[,] obj, DeepCloneState state)
        {
            // not null from called method, but will check it anyway
            if (obj == null) return null;
            var l1 = obj.GetLength(0);
            var l2 = obj.GetLength(1);
            var outArray = new T[l1, l2];
            state.AddKnownRef(obj, outArray);
            if (DeepClonerSafeTypes.CanReturnSameObject(typeof(T)))
            {
                Array.Copy(obj, outArray, obj.Length);
                return outArray;
            }

            if (typeof(T).GetTypeInfo().IsValueType)
            {
                var cloner = GetClonerForValueType<T>();
                for (var i = 0; i < l1; i++)
                    for (var k = 0; k < l2; k++)
                        outArray[i, k] = cloner(obj[i, k], state);
            }
            else
            {
                for (var i = 0; i < l1; i++)
                    for (var k = 0; k < l2; k++)
                        outArray[i, k] = (T)CloneClassInternal(obj[i, k], state);
            }

            return outArray;
        }

        // rare cases, very slow cloning. currently it's ok
        internal static Array CloneAbstractArrayInternal(Array obj, DeepCloneState state)
        {
            // not null from called method, but will check it anyway
            if (obj == null) return null;
            var rank = obj.Rank;

            var lowerBounds = Enumerable.Range(0, rank).Select(obj.GetLowerBound).ToArray();
            var lengths = Enumerable.Range(0, rank).Select(obj.GetLength).ToArray();
            var idxes = Enumerable.Range(0, rank).Select(obj.GetLowerBound).ToArray();

            var outArray = Array.CreateInstance(obj.GetType().GetElementType(), lengths, lowerBounds);
            state.AddKnownRef(obj, outArray);
            while (true)
            {
                outArray.SetValue(CloneClassInternal(obj.GetValue(idxes), state), idxes);
                var ofs = rank - 1;
                while (true)
                {
                    idxes[ofs]++;
                    if (idxes[ofs] >= lowerBounds[ofs] + lengths[ofs])
                    {
                        idxes[ofs] = lowerBounds[ofs];
                        ofs--;
                        if (ofs < 0) return outArray;
                    }
                    else
                        break;
                }
            }
        }

        internal static Func<T, DeepCloneState, T> GetClonerForValueType<T>()
        {
            return (Func<T, DeepCloneState, T>)DeepClonerCache.GetOrAddStructAsObject(typeof(T), t => GenerateCloner(t, false));
        }

        private static object GenerateCloner(Type t, bool asObject)
        {
            if (DeepClonerSafeTypes.CanReturnSameObject(t) && (asObject && !t.IsValueType))
                return null;

			return DeepClonerExprGenerator.GenerateClonerInternal(t, asObject);
        }      
    }

    internal class DeepCloneState
    {
        private class CustomEqualityComparer : IEqualityComparer<object>, IEqualityComparer
        {
            bool IEqualityComparer<object>.Equals(object x, object y)
            {
                return ReferenceEquals(x, y);
            }

            bool IEqualityComparer.Equals(object x, object y)
            {
                return ReferenceEquals(x, y);
            }

            public int GetHashCode(object obj)
            {
                return RuntimeHelpers.GetHashCode(obj);
            }
        }

        private MiniDictionary _loops;

        private readonly object[] _baseFromTo = new object[6];

        private int _idx;

        public object GetKnownRef(object from)
        {
            // this is faster than call Diectionary from begin
            // also, small poco objects does not have a lot of references
            var baseFromTo = _baseFromTo;
            if (ReferenceEquals(from, baseFromTo[0])) return baseFromTo[3];
            if (ReferenceEquals(from, baseFromTo[1])) return baseFromTo[4];
            if (ReferenceEquals(from, baseFromTo[2])) return baseFromTo[5];
            if (_loops == null)
                return null;

            return _loops.FindEntry(from);
        }

        public void AddKnownRef(object from, object to)
        {
            if (_idx < 3)
            {
                _baseFromTo[_idx] = from;
                _baseFromTo[_idx + 3] = to;
                _idx++;
                return;
            }

            if (_loops == null)
                _loops = new MiniDictionary();
            _loops.Insert(from, to);
        }

        private class MiniDictionary
        {
            private struct Entry
            {
                public int HashCode;
                public int Next;
                public object Key;
                public object Value;
            }

            private int[] _buckets;
            private Entry[] _entries;
            private int _count;


            public MiniDictionary() : this(5)
            {
            }

            public MiniDictionary(int capacity)
            {
                if (capacity > 0)
                    Initialize(capacity);
            }

            public object FindEntry(object key)
            {
                if (_buckets != null)
                {
                    var hashCode = RuntimeHelpers.GetHashCode(key) & 0x7FFFFFFF;
                    var entries1 = _entries;
                    for (var i = _buckets[hashCode % _buckets.Length]; i >= 0; i = entries1[i].Next)
                    {
                        if (entries1[i].HashCode == hashCode && ReferenceEquals(entries1[i].Key, key))
                            return entries1[i].Value;
                    }
                }

                return null;
            }

            private static readonly int[] _primes =
            {
                3, 7, 11, 17, 23, 29, 37, 47, 59, 71, 89, 107, 131, 163, 197, 239, 293, 353, 431, 521, 631, 761, 919,
                1103, 1327, 1597, 1931, 2333, 2801, 3371, 4049, 4861, 5839, 7013, 8419, 10103, 12143, 14591,
                17519, 21023, 25229, 30293, 36353, 43627, 52361, 62851, 75431, 90523, 108631, 130363, 156437,
                187751, 225307, 270371, 324449, 389357, 467237, 560689, 672827, 807403, 968897, 1162687, 1395263,
                1674319, 2009191, 2411033, 2893249, 3471899, 4166287, 4999559, 5999471, 7199369
            };

            private static int GetPrime(int min)
            {
                for (var i = 0; i < _primes.Length; i++)
                {
                    var prime = _primes[i];
                    if (prime >= min) return prime;
                }

                //outside of our predefined table. 
                //compute the hard way. 
                for (var i = min | 1; i < int.MaxValue; i += 2)
                {
                    if (IsPrime(i) && (i - 1) % 101 != 0)
                        return i;
                }

                return min;
            }

            private static bool IsPrime(int candidate)
            {
                if ((candidate & 1) != 0)
                {
                    var limit = (int)Math.Sqrt(candidate);
                    for (var divisor = 3; divisor <= limit; divisor += 2)
                    {
                        if ((candidate % divisor) == 0)
                            return false;
                    }

                    return true;
                }

                return candidate == 2;
            }

            private static int ExpandPrime(int oldSize)
            {
                var newSize = 2 * oldSize;

                if ((uint)newSize > 0x7FEFFFFD && 0x7FEFFFFD > oldSize)
                {
                    return 0x7FEFFFFD;
                }

                return GetPrime(newSize);
            }

            private void Initialize(int size)
            {
                _buckets = new int[size];
                for (int i = 0; i < _buckets.Length; i++)
                    _buckets[i] = -1;
                _entries = new Entry[size];
            }

            public void Insert(object key, object value)
            {
                if (_buckets == null) Initialize(0);
                var hashCode = RuntimeHelpers.GetHashCode(key) & 0x7FFFFFFF;
                var targetBucket = hashCode % _buckets.Length;

                var entries1 = _entries;

                // we're always checking for entry before adding new
                // so this loop is useless
                /*for (var i = _buckets[targetBucket]; i >= 0; i = entries1[i].Next)
				{
					if (entries1[i].HashCode == hashCode && ReferenceEquals(entries1[i].Key, key))
					{
						entries1[i].Value = value;
						return;
					}
				}*/

                if (_count == entries1.Length)
                {
                    Resize();
                    entries1 = _entries;
                    targetBucket = hashCode % _buckets.Length;
                }

                var index = _count;
                _count++;

                entries1[index].HashCode = hashCode;
                entries1[index].Next = _buckets[targetBucket];
                entries1[index].Key = key;
                entries1[index].Value = value;
                _buckets[targetBucket] = index;
            }

            private void Resize()
            {
                Resize(ExpandPrime(_count));
            }

            private void Resize(int newSize)
            {
                var newBuckets = new int[newSize];
                for (int i = 0; i < newBuckets.Length; i++)
                    newBuckets[i] = -1;
                var newEntries = new Entry[newSize];
                Array.Copy(_entries, 0, newEntries, 0, _count);

                for (var i = 0; i < _count; i++)
                {
                    if (newEntries[i].HashCode >= 0)
                    {
                        var bucket = newEntries[i].HashCode % newSize;
                        newEntries[i].Next = newBuckets[bucket];
                        newBuckets[bucket] = i;
                    }
                }

                _buckets = newBuckets;
                _entries = newEntries;
            }
        }
    }

    internal static class DeepClonerCache
    {
        private static readonly ConcurrentDictionary<Type, object> _typeCache = new ConcurrentDictionary<Type, object>();

        private static readonly ConcurrentDictionary<Type, object> _typeCacheDeepTo = new ConcurrentDictionary<Type, object>();

        private static readonly ConcurrentDictionary<Type, object> _typeCacheShallowTo = new ConcurrentDictionary<Type, object>();

        private static readonly ConcurrentDictionary<Type, object> _structAsObjectCache = new ConcurrentDictionary<Type, object>();

        private static readonly ConcurrentDictionary<Tuple<Type, Type>, object> _typeConvertCache = new ConcurrentDictionary<Tuple<Type, Type>, object>();

        public static object GetOrAddClass<T>(Type type, Func<Type, T> adder)
        {
            // return _typeCache.GetOrAdd(type, x => adder(x));

            // this implementation is slightly faster than getoradd
            object value;
            if (_typeCache.TryGetValue(type, out value)) return value;
            value = adder(type);
            _typeCache.TryAdd(type, value);
            return value;
        }

        public static object GetOrAddDeepClassTo<T>(Type type, Func<Type, T> adder)
        {
            object value;
            if (_typeCacheDeepTo.TryGetValue(type, out value)) return value;
            value = adder(type);
            _typeCacheDeepTo.TryAdd(type, value);
            return value;
        }

        public static object GetOrAddShallowClassTo<T>(Type type, Func<Type, T> adder)
        {
            object value;
            if (_typeCacheShallowTo.TryGetValue(type, out value)) return value;
            value = adder(type);
            _typeCacheShallowTo.TryAdd(type, value);
            return value;
        }

        public static object GetOrAddStructAsObject<T>(Type type, Func<Type, T> adder)
        {
            // return _typeCache.GetOrAdd(type, x => adder(x));

            // this implementation is slightly faster than getoradd
            object value;
            if (_structAsObjectCache.TryGetValue(type, out value)) return value;
            value = adder(type);
            _structAsObjectCache.TryAdd(type, value);
            return value;
        }

        public static T GetOrAddConvertor<T>(Type from, Type to, Func<Type, Type, T> adder)
        {
            return (T)_typeConvertCache.GetOrAdd(new Tuple<Type, Type>(from, to), (tuple) => adder(tuple.Item1, tuple.Item2));
        }

        /// <summary>
        /// This method can be used when we switch between safe / unsafe variants (for testing)
        /// </summary>
        public static void ClearCache()
        {
            _typeCache.Clear();
            _typeCacheDeepTo.Clear();
            _typeCacheShallowTo.Clear();
            _structAsObjectCache.Clear();
            _typeConvertCache.Clear();
        }
    }    

    internal static class DeepClonerExprGenerator
    {
        internal static object GenerateClonerInternal(Type realType, bool asObject)
        {
            return GenerateProcessMethod(realType, asObject && realType.IsValueType);
        }

        // slow, but hardcore method to set readonly field
        internal static void ForceSetField(FieldInfo field, object obj, object value)
        {
            var fieldInfo = field.GetType().GetTypeInfo().GetDeclaredField("m_fieldAttributes");

            // TODO: think about it
            // nothing to do :( we should a throw an exception, but it is no good for user
            if (fieldInfo == null)
                return;
            var ov = fieldInfo.GetValue(field);
            if (!(ov is FieldAttributes))
                return;
            var v = (FieldAttributes)ov;

            fieldInfo.SetValue(field, v & ~FieldAttributes.InitOnly);
            field.SetValue(obj, value);
            fieldInfo.SetValue(field, v);
        }

        private static object GenerateProcessMethod(Type type, bool unboxStruct)
        {
            if (type.IsArray)
            {
                return GenerateProcessArrayMethod(type);
            }

            if (type.FullName != null && type.FullName.StartsWith("System.Tuple`"))
            {
                // if not safe type it is no guarantee that some type will contain reference to
                // this tuple. In usual way, we're creating new object, setting reference for it
                // and filling data. For tuple, we will fill data before creating object
                // (in constructor arguments)
                var genericArguments = type.GetGenericArguments();
                // current tuples contain only 8 arguments, but may be in future...
                // we'll write code that works with it
                if (genericArguments.Length < 10 && genericArguments.All(DeepClonerSafeTypes.CanReturnSameObject))
                {
                    return GenerateProcessTupleMethod(type);
                }
            }

            var methodType = unboxStruct || type.IsClass ? typeof(object) : type;

            var expressionList = new List<Expression>();

            ParameterExpression from = Expression.Parameter(methodType);
            var fromLocal = from;
            var toLocal = Expression.Variable(type);
            var state = Expression.Parameter(typeof(DeepCloneState));

            if (!type.IsValueType)
            {
                var methodInfo = typeof(object).GetPrivateMethod("MemberwiseClone");

                // to = (T)from.MemberwiseClone()
                expressionList.Add(Expression.Assign(toLocal, Expression.Convert(Expression.Call(from, methodInfo), type)));

                fromLocal = Expression.Variable(type);
                // fromLocal = (T)from
                expressionList.Add(Expression.Assign(fromLocal, Expression.Convert(from, type)));

                // added from -> to binding to ensure reference loop handling
                // structs cannot loop here
                // state.AddKnownRef(from, to)
                expressionList.Add(Expression.Call(state, typeof(DeepCloneState).GetMethod("AddKnownRef"), from, toLocal));
            }
            else
            {
                if (unboxStruct)
                {
                    // toLocal = (T)from;
                    expressionList.Add(Expression.Assign(toLocal, Expression.Unbox(from, type)));
                    fromLocal = Expression.Variable(type);
                    // fromLocal = toLocal; // structs, it is ok to copy
                    expressionList.Add(Expression.Assign(fromLocal, toLocal));
                }
                else
                {
                    // toLocal = from
                    expressionList.Add(Expression.Assign(toLocal, from));
                }
            }

            List<FieldInfo> fi = new List<FieldInfo>();
            var tp = type;
            do
            {
				if (tp.Name == "ContextBoundObject") break;

                fi.AddRange(tp.GetTypeInfo().DeclaredFields.Where(x => !x.IsStatic).ToArray());
                tp = tp.BaseType;
            }
            while (tp != null);

            foreach (var fieldInfo in fi)
            {
                if (!DeepClonerSafeTypes.CanReturnSameObject(fieldInfo.FieldType))
                {
                    var methodInfo = fieldInfo.FieldType.IsValueType
                                        ? typeof(DeepClonerGenerator).GetPrivateStaticMethod("CloneStructInternal")
                                                                    .MakeGenericMethod(fieldInfo.FieldType)
                                        : typeof(DeepClonerGenerator).GetPrivateStaticMethod("CloneClassInternal");

                    var get = Expression.Field(fromLocal, fieldInfo);

                    // toLocal.Field = Clone...Internal(fromLocal.Field)
                    var call = (Expression)Expression.Call(methodInfo, get, state);
                    if (!fieldInfo.FieldType.IsValueType)
                        call = Expression.Convert(call, fieldInfo.FieldType);

                    // should handle specially
                    // todo: think about optimization, but it rare case
                    if (fieldInfo.IsInitOnly)
                    {
                        // var setMethod = fieldInfo.GetType().GetMethod("SetValue", new[] { typeof(object), typeof(object) });
                        // expressionList.Add(Expression.Call(Expression.Constant(fieldInfo), setMethod, toLocal, call));
                        var setMethod = typeof(DeepClonerExprGenerator).GetPrivateStaticMethod("ForceSetField");
                        expressionList.Add(Expression.Call(setMethod, Expression.Constant(fieldInfo), Expression.Convert(toLocal, typeof(object)), Expression.Convert(call, typeof(object))));
                    }
                    else
                    {
                        expressionList.Add(Expression.Assign(Expression.Field(toLocal, fieldInfo), call));
                    }
                }
            }

            expressionList.Add(Expression.Convert(toLocal, methodType));

            var funcType = typeof(Func<,,>).MakeGenericType(methodType, typeof(DeepCloneState), methodType);

            var blockParams = new List<ParameterExpression>();
            if (from != fromLocal) blockParams.Add(fromLocal);
            blockParams.Add(toLocal);

            return Expression.Lambda(funcType, Expression.Block(blockParams, expressionList), from, state).Compile();
        }

        private static object GenerateProcessArrayMethod(Type type)
        {
            var elementType = type.GetElementType();
            var rank = type.GetArrayRank();

            MethodInfo methodInfo;

            // multidim or not zero-based arrays
            if (rank != 1 || type != elementType.MakeArrayType())
            {
                if (rank == 2 && type == elementType.MakeArrayType())
                {
                    // small optimization for 2 dim arrays
                    methodInfo = typeof(DeepClonerGenerator).GetPrivateStaticMethod("Clone2DimArrayInternal").MakeGenericMethod(elementType);
                }
                else
                {
                    methodInfo = typeof(DeepClonerGenerator).GetPrivateStaticMethod("CloneAbstractArrayInternal");
                }
            }
            else
            {
                var methodName = "Clone1DimArrayClassInternal";
                if (DeepClonerSafeTypes.CanReturnSameObject(elementType)) methodName = "Clone1DimArraySafeInternal";
                else if (elementType.IsValueType) methodName = "Clone1DimArrayStructInternal";
                methodInfo = typeof(DeepClonerGenerator).GetPrivateStaticMethod(methodName).MakeGenericMethod(elementType);
            }

            ParameterExpression from = Expression.Parameter(typeof(object));
            var state = Expression.Parameter(typeof(DeepCloneState));
            var call = Expression.Call(methodInfo, Expression.Convert(from, type), state);

            var funcType = typeof(Func<,,>).MakeGenericType(typeof(object), typeof(DeepCloneState), typeof(object));

            return Expression.Lambda(funcType, call, from, state).Compile();
        }

        private static object GenerateProcessTupleMethod(Type type)
        {
            ParameterExpression from = Expression.Parameter(typeof(object));
            var state = Expression.Parameter(typeof(DeepCloneState));

            var local = Expression.Variable(type);
            var assign = Expression.Assign(local, Expression.Convert(from, type));

            var funcType = typeof(Func<object, DeepCloneState, object>);

            var tupleLength = type.GetTypeInfo().GenericTypeArguments.Length;

            var constructor = Expression.Assign(local, Expression.New(type.GetTypeInfo().DeclaredConstructors.ToArray().First(x => x.GetParameters().Length == tupleLength),
                type.GetTypeInfo().DeclaredProperties.ToArray().OrderBy(x => x.Name)
                    .Where(x => x.CanRead && x.Name.StartsWith("Item") && char.IsDigit(x.Name[4]))
                    .Select(x => Expression.Property(local, x.Name))));

            return Expression.Lambda(funcType, Expression.Block(new[] { local },
                assign, constructor, Expression.Call(state, typeof(DeepCloneState).GetMethod("AddKnownRef"), from, local),
                    from),
                from, state).Compile();
        }
    }

	internal static class DeepClonerSafeTypes
    {
        internal static readonly ConcurrentDictionary<Type, bool> KnownTypes = new ConcurrentDictionary<Type, bool>();

        static DeepClonerSafeTypes()
        {
            foreach (
                var x in
                    new[]
                        {
                            typeof(byte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong),
                            typeof(float), typeof(double), typeof(decimal), typeof(char), typeof(string), typeof(bool), typeof(DateTime),
                            typeof(IntPtr), typeof(UIntPtr), typeof(Guid),
							// do not clone such native type
							Type.GetType("System.RuntimeType"),
                            Type.GetType("System.RuntimeTypeHandle"),
						}) KnownTypes.TryAdd(x, true);
        }

        private static bool CanReturnSameType(Type type, HashSet<Type> processingTypes)
        {
            if (KnownTypes.TryGetValue(type, out bool isSafe))
                return isSafe;

            // enums are safe
            // pointers (e.g. int*) are unsafe, but we cannot do anything with it except blind copy
            if (type.IsEnum || type.IsPointer)
            {
                KnownTypes.TryAdd(type, true);
                return true;
            }

			// do not copy db null
			if (type.FullName.StartsWith("System.DBNull"))
			{
				KnownTypes.TryAdd(type, true);
				return true;
			}

			if (type.FullName.StartsWith("System.RuntimeType"))
			{
				KnownTypes.TryAdd(type, true);
				return true;
			}
			
			if (type.FullName.StartsWith("System.Reflection.") && Equals(type.GetTypeInfo().Assembly, typeof(PropertyInfo).GetTypeInfo().Assembly))
			{
				KnownTypes.TryAdd(type, true);
				return true;
			}

			if (type.IsSubclassOfTypeByName("CriticalFinalizerObject"))
			{
				KnownTypes.TryAdd(type, true);
				return true;
			}
			
			// better not to touch ms dependency injection
			if (type.FullName.StartsWith("Microsoft.Extensions.DependencyInjection."))
			{
				KnownTypes.TryAdd(type, true);
				return true;
			}

			if (type.FullName == "Microsoft.EntityFrameworkCore.Internal.ConcurrencyDetector")
			{
				KnownTypes.TryAdd(type, true);
				return true;
			}

            // classes are always unsafe (we should copy it fully to count references)
            if (!type.IsValueType)
            {
                KnownTypes.TryAdd(type, false);
                return false;
            }

            if (processingTypes == null)
                processingTypes = new HashSet<Type>();

            // structs cannot have a loops, but check it anyway
            processingTypes.Add(type);

            List<FieldInfo> fi = new List<FieldInfo>();
            var tp = type;
            do
            {
                fi.AddRange(tp.GetTypeInfo().DeclaredFields.Where(x => !x.IsStatic).ToArray());
                tp = tp.BaseType;
            }
            while (tp != null);

            foreach (var fieldInfo in fi)
            {
                // type loop
                var fieldType = fieldInfo.FieldType;
                if (processingTypes.Contains(fieldType))
                    continue;

                // not safe and not not safe. we need to go deeper
                if (!CanReturnSameType(fieldType, processingTypes))
                {
                    KnownTypes.TryAdd(type, false);
                    return false;
                }
            }

            KnownTypes.TryAdd(type, true);
            return true;
        }

        public static bool CanReturnSameObject(Type type)
        {
            return CanReturnSameType(type, null);
        }
    }    
}
