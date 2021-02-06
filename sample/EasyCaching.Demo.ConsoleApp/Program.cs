namespace EasyCaching.Demo.ConsoleApp
{
    using EasyCaching.Core;
    using EasyCaching.SQLite;
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
                option.UseInMemory("m1");

                //option.UseRedis(config =>
                //{
                //    config.DBConfig = new Redis.RedisDBOptions { Configuration = "localhost" };
                //    config.SerializerName = "json";
                //}, "r1");


                option.UseSQLite(c =>
                {
                    c.DBConfig = new SQLiteDBOptions
                    {
                        FileName = "demo.db",
                        CacheMode = Microsoft.Data.Sqlite.SqliteCacheMode.Default,
                        OpenMode = Microsoft.Data.Sqlite.SqliteOpenMode.Memory,
                    };
                }, "s1");

                //option.WithJson(jsonSerializerSettingsConfigure: x =>
                //{
                //    x.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.None;
                //}, "json");
            });

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            var factory = serviceProvider.GetService<IEasyCachingProviderFactory>();

            //var redisCache = factory.GetCachingProvider("r1");

            //redisCache.Set<Product>("rkey", new Product() { Name = "test" }, TimeSpan.FromSeconds(20));

            //var redisVal = redisCache.Get<Product>("rkey");

            //Console.WriteLine($"redis cache get value, {redisVal.HasValue} {redisVal.IsNull} {redisVal.Value} ");


            var mCache = factory.GetCachingProvider("m1");

            mCache.Set<Product>("mkey1", new Product() { Name = "test" }, TimeSpan.FromSeconds(20));

            var mVal1 = mCache.Get<Product>("mkey1");


            mCache.Set<string>("mkey", "mvalue", TimeSpan.FromSeconds(20));

            var mVal = mCache.Get<string>("mkey");

            Console.WriteLine($"in-memory cache get value, {mVal.HasValue} {mVal.IsNull} {mVal.Value} ");

            var sCache = factory.GetCachingProvider("s1");

            sCache.Set<string>("skey", "svalue", TimeSpan.FromSeconds(20));

            var sVal = sCache.Get<string>("skey");

            Console.WriteLine($"sqlite cache get value, {sVal.HasValue} {sVal.IsNull} {sVal.Value} ");

            Console.ReadKey();
        }
    }

    public class Product
    {

        public string Name { get; set; }
    }
}
