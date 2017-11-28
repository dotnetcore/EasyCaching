namespace EasyCaching.Demo.Interceptor.Services
{
    public interface IDateTimeService : EasyCaching.Core.Internal.IEasyCaching
    {
        [EasyCaching.Extensions.CachingInterceptor(AbsoluteExpiration = 10)]
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
