namespace EasyCaching.Demo.Interceptors.Services
{
    using System.Threading.Tasks;
    using EasyCaching.Core.Interceptor;
    using ProtoBuf;

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
        Task<System.Collections.Generic.List<Demo>> GetDemoListAsync(int id);
        

        [EasyCachingAble(Expiration = 10)]
        Demo GetDemo(int id);

        [EasyCachingAble(Expiration = 10)]
        object GetData();
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

        public async Task DeleteSomethingAsync(int id)
        {
            await Task.Run(() => System.Console.WriteLine("Handle delete something.."));
        }

        public string PutSomething(string str)
        {
            return str;
        }

        public object GetData()
        {
            return new { x = System.DateTimeOffset.Now.ToUnixTimeSeconds() };
        }
    }

    [ProtoContract]
    [System.Serializable]
    public class Demo
    {
        [ProtoMember(1)]
        public int Id { get; set; }
        [ProtoMember(2)]
        public string Name { get; set; }
        [ProtoMember(3)]
        public System.DateTime CreateTime { get; set; }
    }
}
