namespace EasyCaching.Demo.Interceptors.Services
{    
    using System.Threading.Tasks;
    using EasyCaching.Core.Interceptor;
    using EasyCaching.Core.Internal;

    public interface ICastleService
    {
        [EasyCachingAble(Expiration = 10)]
        string GetCurrentUtcTime();

        [EasyCachingPut(CacheKeyPrefix = "Castle")]
        string PutSomething(string str);

        [EasyCachingEvict(IsBefore = true)]
        void DeleteSomething(int id);

        [EasyCachingAble(Expiration = 10)]
        Task<string> GetUtcTimeAsync();

        [EasyCachingAble(Expiration = 10)]
        Task<Demo> GetDemoAsync(int id);

        [EasyCachingAble(Expiration = 10)]
        Task<System.Collections.Generic.List<Demo>> GetDemoListAsync(int id);

        [EasyCachingAble(Expiration = 10)]
        Demo GetDemo(int id);
    }

    public class CastleService : ICastleService//, IEasyCaching
    {
        public void DeleteSomething(int id)
        {
            System.Console.WriteLine("Handle delete something..");
        }

        public string GetCurrentUtcTime()
        {
            return System.DateTime.UtcNow.ToString();
        }

        public Demo GetDemo(int id)
        {
            return new Demo { Id = id, CreateTime = System.DateTime.Now, Name = "catcher" };
        }

        public Task<Demo> GetDemoAsync(int id)
        {
            return Task.FromResult(new Demo { Id = id, CreateTime = System.DateTime.Now, Name = "catcher" });
        }

        public Task<System.Collections.Generic.List<Demo>> GetDemoListAsync(int id)
        {
            return Task.FromResult(new System.Collections.Generic.List<Demo>() { new Demo { Id = id, CreateTime = System.DateTime.Now, Name = "catcher" } });
        }

        public async Task<string> GetUtcTimeAsync()
        {
            return await Task.FromResult<string>(System.DateTimeOffset.UtcNow.ToString());
        }

        public string PutSomething(string str)
        {
            return str;
        }
    }
}
