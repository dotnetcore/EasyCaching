namespace EasyCaching.Demo.Interceptors.Services
{
    using System.Threading.Tasks;
    using EasyCaching.Core.Interceptor;

    public interface IAspectCoreService //: EasyCaching.Core.Internal.IEasyCaching
    {
        [EasyCachingAble(Expiration = 10)]
        string GetCurrentUtcTime();

        [EasyCachingPut(CacheKeyPrefix = "AspectCore")]
        string PutSomething(string str);

        [EasyCachingEvict(IsBefore = true)]
        void DeleteSomething(int id);

        [EasyCachingAble(Expiration = 10)]
        Task<string> GetUtcTimeAsync();

        [EasyCachingAble(Expiration = 10)]
        Task<Demo> GetDemoAsync(int id);

        [EasyCachingAble(Expiration = 10)]
        Demo GetDemo(int id);
    }

    public class AspectCoreService : IAspectCoreService
    {
        public void DeleteSomething(int id)
        {
            System.Console.WriteLine("Handle delete something..");
        }

        public string GetCurrentUtcTime()
        {
            return System.DateTimeOffset.UtcNow.ToString();
        }

        public Demo GetDemo(int id)
        {
             return new Demo { Id = id, CreateTime = System.DateTime.Now, Name = "catcher" };
        }

        public Task<Demo> GetDemoAsync(int id)
        {
            return Task.FromResult(new Demo{ Id = id, CreateTime = System.DateTime.Now, Name = "catcher"});
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

    [System.Serializable]
    public class Demo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public System.DateTime CreateTime { get; set; }
    }
}
