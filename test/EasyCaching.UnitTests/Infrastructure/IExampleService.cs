namespace EasyCaching.UnitTests.Infrastructure
{
    using EasyCaching.Core.Internal;
    using System;
    using Xunit;

    public interface ICastleExampleService
    {
        string GetCurrentUTC();
    }

    public class CastleExampleService : ICastleExampleService ,  IEasyCaching
    {
        [EasyCachingInterceptor(Expiration = 1)]
        public string GetCurrentUTC()
        {
            return DateTime.UtcNow.ToString();
        }
    }

    public interface IAspectCoreExampleService :  IEasyCaching
    {
        [EasyCachingInterceptor(Expiration = 1)]
        string GetCurrentUTC();
    }

    public class AspectCoreExampleService : IAspectCoreExampleService 
    {        
        public string GetCurrentUTC()
        {
            return DateTime.UtcNow.ToString();
        }
    }
}
