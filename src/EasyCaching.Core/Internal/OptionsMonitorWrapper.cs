namespace EasyCaching.Core.Internal
{
    using System;
    using Microsoft.Extensions.Options;

    public class OptionsMonitorWrapper<T> : IOptionsMonitor<T>
    {
        private T _option;

        public OptionsMonitorWrapper(T option)
        {
            _option = option;
        }

        public T CurrentValue => _option;

        public T Get(string name)
        {
            return _option;
        }

        public IDisposable OnChange(Action<T, string> listener)
        {
            return null;
        }
    }
}
