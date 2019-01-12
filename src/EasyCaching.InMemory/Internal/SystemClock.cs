namespace EasyCaching.InMemory
{
    using System;

    internal interface ISystemClock
    {
        DateTimeOffset UtcNow();
    }

    internal class DefaultSystemClock : ISystemClock
    {
        public static readonly DefaultSystemClock Instance = new DefaultSystemClock();

        public DateTimeOffset UtcNow() => DateTimeOffset.UtcNow;
    }

    internal static class SystemClock
    {
        private static ISystemClock _instance = DefaultSystemClock.Instance;
        public static ISystemClock Instance
        {
            get => _instance ?? DefaultSystemClock.Instance;
            set => _instance = value;
        }

        public static DateTimeOffset UtcNow => Instance.UtcNow();
    }
}
