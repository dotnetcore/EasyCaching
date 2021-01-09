namespace EasyCaching.Core.Decoration
{
    using System;
    using System.Threading;

    public class LazyWithoutExceptionCaching<T>
    {
        private readonly Func<T> _factory;

        private T _value;
        private object _lock;
        private bool _initialized;

        public LazyWithoutExceptionCaching(Func<T> factory)
        {
            _factory = factory;
        }

        public bool Initialized => _initialized;

        public T Value => LazyInitializer.EnsureInitialized(ref _value, ref _initialized, ref _lock, _factory);
    }
}