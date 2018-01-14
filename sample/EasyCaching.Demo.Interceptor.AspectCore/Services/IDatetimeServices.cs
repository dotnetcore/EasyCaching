namespace EasyCaching.Demo.Interceptor.AspectCore.Services
{
    using EasyCaching.Core.Internal;
    
    public interface IDateTimeService : EasyCaching.Core.Internal.IEasyCaching
    {
        [EasyCachingInterceptor(Expiration = 10)]
        string GetCurrentUtcTime();
    }

    public class DateTimeService : IDateTimeService
    {        
        public string GetCurrentUtcTime()
        {
            return System.DateTime.UtcNow.ToString();
        }
    }
}
