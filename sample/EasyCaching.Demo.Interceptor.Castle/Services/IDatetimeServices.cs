namespace EasyCaching.Demo.Interceptor.Castle
{
    using EasyCaching.Core.Internal;
    
    public interface IDateTimeService 
    {        
        string GetCurrentUtcTime();

        string PutSomething(string str);

        void DeleteSomething(int id);
    }

    public class DateTimeService : IDateTimeService ,  IEasyCaching
    {
        [EasyCachingEvict(IsBefore = true)]
        public void DeleteSomething(int id)
        {
            System.Console.WriteLine("Handle delete something..");
        }

        [EasyCachingAble(Expiration = 10)]
        public string GetCurrentUtcTime()
        {
            return System.DateTime.UtcNow.ToString();
        }

        [EasyCachingPut(CacheKeyPrefix = "Castle")]
        public string PutSomething(string str)
        {
            return str;
        }
    }
}
