namespace EasyCaching.UnitTests.Infrastructure
{
    using EasyCaching.Core.Internal;
    using System;

    public interface ICastleExampleService
    {
        string GetCurrentUTC();

        long GetCurrentUTCTick();
    }

    public class CastleExampleService : ICastleExampleService, IEasyCaching
    {
        [EasyCachingInterceptor(Expiration = 1)]
        public string GetCurrentUTC()
        {
            return DateTime.UtcNow.ToString();
        }

        public long GetCurrentUTCTick()
        {
            return DateTime.UtcNow.Ticks;
        }
    }

    public interface IAspectCoreExampleService : IEasyCaching
    {
        [EasyCachingInterceptor(Expiration = 1)]
        string GetCurrentUTC();

        long GetCurrentUTCTick();
    }

    public class AspectCoreExampleService : IAspectCoreExampleService
    {
        public string GetCurrentUTC()
        {
            return DateTime.UtcNow.ToString();
        }

        public long GetCurrentUTCTick()
        {
            return DateTime.UtcNow.Ticks;
        }
    }
}