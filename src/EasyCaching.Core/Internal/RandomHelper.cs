namespace EasyCaching.Core.Internal
{
    using System;

    public static class RandomHelper
    {
#if NETSTANDARD2_0
        private static readonly Random _random = new Random();
#endif

        public static int GetNext(int min, int max)
        {
#if NET6_0_OR_GREATER
            return Random.Shared.Next(min, max);
#else
            return _random.Next(min, max);
#endif
        }
    }
}
