namespace EasyCaching.Demo.Providers
{
    using EasyCaching.Core;
    using EasyCaching.Core.Internal;
    using EasyCaching.InMemory;
    using EasyCaching.HybridCache;
    using EasyCaching.Memcached;
    using EasyCaching.Redis;
    using EasyCaching.SQLite;
    using EasyCaching.Serialization.MessagePack;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using System;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            //new configuration
            services.AddEasyCaching(option=> 
            {
                //use memory cache
                option.UseInMemory("default");

                //use memory cache
                option.UseInMemory("cus");

                //use redis cache
                option.UseRedis(config => 
                {
                    config.DBConfig.Endpoints.Add(new ServerEndPoint("127.0.0.1", 6379));
                }, "redis1")
                .WithMessagePack()//with messagepack serialization
                ;

                //use redis cache
                option.UseRedis(config => 
                {
                    config.DBConfig.Endpoints.Add(new ServerEndPoint("127.0.0.1", 6380));
                }, "redis2");

                ////use sqlite cache
                //option.UseSQLite(config =>
                //{
                //    config.DBConfig = new SQLiteDBOptions { FileName = "my.db" };
                //});

                ////use memcached cached
                //option.UseMemcached(config => 
                //{
                //    config.DBConfig.AddServer("127.0.0.1", 11211);
                //});

                //option.UseMemcached(Configuration);

            });

            //1. Important step for using InMemory Cache
            //services.AddDefaultInMemoryCache();

            //services.AddDefaultInMemoryCacheWithFactory();
            //services.AddDefaultInMemoryCacheWithFactory("cus");

            //services.AddDefaultInMemoryCache(Configuration);

            ////2. Important step for using Memcached Cache
            //services.AddDefaultMemcached(op =>
            //{
            //    op.DBConfig.AddServer("127.0.0.1", 11211);
            //});

            //services.AddDefaultMemcached(Configuration);

            //3. Important step for using Redis Cache
            //services.AddDefaultRedisCache(option =>
            //{
            //    option.DBConfig.Endpoints.Add(new ServerEndPoint("127.0.0.1", 6379));
            //    option.DBConfig.Password = "";
            //});

            //services.AddDefaultRedisCacheWithFactory("redis1",option =>
            //{
            //    option.DBConfig.Endpoints.Add(new ServerEndPoint("127.0.0.1", 6379));
            //    option.DBConfig.Password = "";
            //});

            //services.AddDefaultRedisCacheWithFactory("redis2", option =>
            //{
            //    option.DBConfig.Endpoints.Add(new ServerEndPoint("127.0.0.1", 6380));
            //    option.DBConfig.Password = "";
            //});

            //services.AddDefaultRedisCache(Configuration);

            ////4. Important step for using SQLite Cache
            //services.AddSQLiteCache(option => 
            //{
            //    option.DBConfig = new SQLiteDBOptions { FileName="my.db" };
            //});

            //services.AddSQLiteCache(Configuration);

            ////5. Important step for using Hybrid Cache
            ////5.1. Local Cache
            //services.AddDefaultInMemoryCache(x=>
            //{
            //    x.Order = 1;
            //});
            ////5.2 Distributed Cache
            //services.AddDefaultRedisCache(option =>
            //{
            //    option.Order = 2;
            //    option.DBConfig.Endpoints.Add(new ServerEndPoint("127.0.0.1", 6379));
            //    option.DBConfig.Password = "";
            //});
            ////5.3 Hybrid
            //services.AddDefaultHybridCache();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            loggerFactory.AddConsole(Configuration.GetSection("Logging"));

            ////2. Important step for using Memcached Cache
            //app.UseDefaultMemcached();

            ////4. Important step for using SQLite Cache
            //app.UseSQLiteCache();

            app.UseMvc();
        }
    }
}
