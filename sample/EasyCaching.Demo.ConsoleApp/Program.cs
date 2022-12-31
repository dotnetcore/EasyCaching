using EasyCaching.Serialization.MemoryPack;

namespace EasyCaching.Demo.ConsoleApp
{
    using EasyCaching.Core;
    using EasyCaching.SQLite;
    using MemoryPack;
    using Microsoft.Extensions.DependencyInjection;
    using System;

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to EasyCaching World!");

            IServiceCollection services = new ServiceCollection();
            services.AddEasyCaching(option =>
            {
                option.WithMemoryPack(configure =>
                {
                }, "mempack");

                option.UseInMemory("m1");

                option.UseRedis((options) =>
                {
                    options.SerializerName = "mempack";
                    options.DBConfig.Endpoints.Add(new Core.Configurations.ServerEndPoint("localhost", 6388));
                }, "r1");

                option.UseSQLite(c =>
                {
                    c.DBConfig = new SQLiteDBOptions
                    {
                        FileName = "demo.db",
                        CacheMode = Microsoft.Data.Sqlite.SqliteCacheMode.Default,
                        OpenMode = Microsoft.Data.Sqlite.SqliteOpenMode.Memory,
                    };
                }, "s1");

                // option.WithJson(jsonSerializerSettingsConfigure: x =>
                // {
                //     x.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.None;
                // }, "json");
            });

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            var factory = serviceProvider.GetService<IEasyCachingProviderFactory>();

            // var redisCache = factory.GetCachingProvider("r1");
            //
            // redisCache.Set<Product>("rkey", new Product() { Name = "test" }, TimeSpan.FromSeconds(20));
            //
            // var redisAllKey = redisCache.GetAllKeysByPrefix("rkey");
            //
            // var redisVal = redisCache.Get<Product>("rkey");
            //
            // Console.WriteLine($"redis cache get value, {redisVal.HasValue} {redisVal.IsNull} {redisVal.Value}");



            var rCache = factory.GetCachingProvider("r1");

            var prod = new Product()
            {
                Name = "Name1",
                Lastname = "Lastname1",
                Inner = new()
                {
                    Name = "Name2",
                    Lastname = "Lastname2"
                }
            };

            prod.Inner.Inner = prod;
            rCache.Set<Product>("mkey1", prod, TimeSpan.FromSeconds(20));

            var mVal1 = rCache.Get<Product>("mkey1");

            rCache.Set<string>("mkey", "mvalue", TimeSpan.FromSeconds(20));

            var mVal = rCache.Get<string>("mkey");

            var mAllKey = rCache.GetAllKeysByPrefix("mk");

            Console.WriteLine($"in-memory cache get value, {mVal.HasValue} {mVal.IsNull} {mVal.Value} ");


            var sCache = factory.GetCachingProvider("s1");


            sCache.Set<string>("skey", "svalue", TimeSpan.FromSeconds(20));


            var sVal = sCache.Get<string>("skey");


            Console.WriteLine($"sqlite cache get value, {sVal.HasValue} {sVal.IsNull} {sVal.Value} ");

            Console.ReadKey();
        }
    }

    [MemoryPackable(GenerateType.CircularReference)]
    public partial class Product
    {

        [MemoryPackOrder(0)]
        public string Name { get; set; }

        [MemoryPackOrder(1)]

        public string Lastname { get; set; }

        [MemoryPackOrder(2)]

        public Product Inner { set; get; }
    }
}

