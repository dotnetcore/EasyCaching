//using EasyCaching.Core;
//using EasyCaching.Core.Configurations;
//using EasyCaching.Core.DistributedLock;
//using EasyCaching.Redis;
//using EasyCaching.Redis.DistributedLock;
//using Google.Protobuf.WellKnownTypes;
//using Microsoft.Extensions.DependencyInjection;
//using Newtonsoft.Json;
//using Xunit.Abstractions;

//namespace EasyCaching.UnitTests.DistributedLock
//{
//    public class EtcdLockTest : BaseDistributedLockTest
//    {
//        private static readonly IDistributedLockFactory Factory = new ServiceCollection()
//            .AddLogging()
//            .AddEasyCaching(option=>option.UseEtcd(options =>
//            {
//                options.Address = "http://127.0.0.1:2379";
//                options.Timeout = 3000;
//                options.LockMs = 10000;
//                options.SerializerName = "json";
//            }).WithJson(jsonSerializerSettingsConfigure: x =>
//            {
//                x.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.None;
//                x.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
//            }, "json").UseEtcdLock())
//            .BuildServiceProvider()
//            .GetService<RedisLockFactory>();

//        public EtcdLockTest(ITestOutputHelper output) : base(EasyCachingConstValue.DefaultEtcdName, Factory, output) { }
//    }
//}
