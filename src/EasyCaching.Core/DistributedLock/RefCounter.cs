using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace EasyCaching.Core.DistributedLock
{
    internal class RefCounter<T>
    {
        private int _refCount = 1;

        public RefCounter(T value) => Value = value;

        public T Value { get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Increment() => Interlocked.Increment(ref _refCount);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Decrement() => Interlocked.Decrement(ref _refCount);
    }

    internal class RefCounterPool<TKey, TValue> where TValue : class
    {
        private readonly IDictionary<TKey, RefCounter<TValue>> _dictionary;

        public RefCounterPool() => _dictionary = new Dictionary<TKey, RefCounter<TValue>>();

        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            if (valueFactory == null) throw new ArgumentNullException(nameof(valueFactory));

            RefCounter<TValue> item;
            lock (_dictionary)
            {
                if (!_dictionary.TryGetValue(key, out item))
                    return (_dictionary[key] = new RefCounter<TValue>(valueFactory(key))).Value;
            }

            item.Increment();

            return item.Value;
        }

        public TValue TryRemove(TKey key)
        {
            RefCounter<TValue> item;

            lock (_dictionary)
            {
                if (!_dictionary.TryGetValue(key, out item) || item.Decrement() > 0) return null;

                _dictionary.Remove(key);
            }

            return item.Value;
        }
    }
}
