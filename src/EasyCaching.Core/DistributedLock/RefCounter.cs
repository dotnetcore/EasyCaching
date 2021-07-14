using System;
using System.Collections.Generic;
using System.Threading;

namespace EasyCaching.Core.DistributedLock
{
    internal class RefCounter<T>
    {
        private int _refCount = 1;

        public RefCounter(T value) => Value = value;

        public T Value { get; }

        public int Increment() => Interlocked.Increment(ref _refCount);
        public int Decrement() => Interlocked.Decrement(ref _refCount);
    }

    public class RefCounterPool<TKey, TValue> where TValue : class
    {
        private readonly IDictionary<TKey, RefCounter<TValue>> _dictionary;

        public RefCounterPool() => _dictionary = new Dictionary<TKey, RefCounter<TValue>>();

        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            if (valueFactory == null) throw new ArgumentNullException(nameof(valueFactory));

            lock (_dictionary)
            {
                if (_dictionary.TryGetValue(key, out var item))
                    item.Increment();
                else
                    item = _dictionary[key] = new RefCounter<TValue>(valueFactory(key));

                return item.Value;
            }
        }

        /// <summary>减少引用，如果没有引用了，则返回原来的值</summary>
        public TValue TryRemove(TKey key)
        {
            RefCounter<TValue> item;
            lock (_dictionary)
                if (_dictionary.TryGetValue(key, out item) && item.Decrement() == 0)
                    _dictionary.Remove(key);
                else
                    return null;

            return item.Value;
        }
    }
}
