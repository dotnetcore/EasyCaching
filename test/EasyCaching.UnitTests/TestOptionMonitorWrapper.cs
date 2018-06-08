namespace EasyCaching.UnitTests
{
    using Microsoft.Extensions.Options;
    using System;

    public class TestOptionMonitorWrapper<T> : IOptionsMonitor<T>
    {
        private T _opt;

        public TestOptionMonitorWrapper(T opt)
        {
            _opt = opt;
        }

        public T CurrentValue => _opt;

        public T Get(string name)
        {
            return _opt;
        }

        public IDisposable OnChange(Action<T, string> listener)
        {
            throw new NotImplementedException();
        }
    }
}
