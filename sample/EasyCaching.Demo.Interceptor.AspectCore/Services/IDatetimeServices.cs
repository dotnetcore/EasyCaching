namespace EasyCaching.Demo.Interceptor.AspectCore.Services
{
    using EasyCaching.Core.Internal;
    
    public interface IDateTimeService : EasyCaching.Core.Internal.IEasyCaching
    {
        [EasyCachingAble(Expiration = 10)]
        string GetCurrentUtcTime();

        [EasyCachingPut(CacheKeyPrefix = "AspectCore")]
        string PutSomething(string str);

        [EasyCachingEvict(IsBefore = true)]
        void DeleteSomething(int id);
    }

    public class DateTimeService : IDateTimeService
    {
        public void DeleteSomething(int id)
        {
            System.Console.WriteLine("Handle delete something..");
        }

        public string GetCurrentUtcTime()
        {
            return System.DateTime.UtcNow.ToString();
        }

        public string PutSomething(string str)
        {
            return str;
        }
    }
}
