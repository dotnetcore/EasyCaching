namespace EasyCaching.Demo.Interceptor.Castle
{
    using EasyCaching.Core.Internal;
    
    public interface IDateTimeService 
    {        
        string GetCurrentUtcTime();
    }

    public class DateTimeService : IDateTimeService ,  IEasyCaching
    {        
        [EasyCachingInterceptor(Expiration = 10)]
        public string GetCurrentUtcTime()
        {
            return System.DateTime.UtcNow.ToString();
        }
    }
}
