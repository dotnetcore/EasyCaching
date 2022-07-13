namespace EasyCaching.Demo.Providers
{
    using EasyCaching.Core.Configurations;
    using EasyCaching.SQLite;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            //new configuration
            services.AddEasyCaching(option =>
            {
                //use memory cache
                option.UseInMemory("default");

                //use memory cache
                option.UseInMemory("cus");

                ////use redis cache
                //option.UseRedis(config =>
                //{
                //    config.DBConfig.Endpoints.Add(new ServerEndPoint("127.0.0.1", 6379));
                //    config.DBConfig.SyncTimeout = 10000;
                //    config.DBConfig.AsyncTimeout = 10000;
                //    config.SerializerName = "mymsgpack";
                //}, "redis1")
                //.WithMessagePack("mymsgpack")//with messagepack serialization
                //;

                ////use redis cache
                //option.UseRedis(config =>
                //{
                //    config.DBConfig.Endpoints.Add(new ServerEndPoint("127.0.0.1", 6380));
                //}, "redis2");

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

                //kafka bus
                //use redis cache
                option.UseRedis(config =>
                {
                    config.DBConfig.Endpoints.Add(new ServerEndPoint("192.168.99.100", 6379));
                    config.DBConfig.SyncTimeout = 10000;
                    config.DBConfig.AsyncTimeout = 10000;
                    config.SerializerName = "mymsgpack";
                }, "myredis")
                .WithMessagePack("mymsgpack")//with messagepack serialization
                ;

                //  使用hybird
                option.UseHybrid(config =>
                {
                    config.EnableLogging = false;
                    // 缓存总线的订阅主题
                    config.TopicName = "MyTestBusTp";
                    // 本地缓存的名字
                    config.LocalCacheProviderName = "cus";
                    // 分布式缓存的名字
                    config.DistributedCacheProviderName = "myredis";
                });

                //读取配置文件的
                //option.WithConfluentKafkaBus(Configuration);
                //直接配置
                option.WithConfluentKafkaBus(x => {
                    x.BootstrapServers = "192.168.99.100:9093";
                    x.GroupId = "MyGroupId";
                });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //loggerFactory.AddConsole(Configuration.GetSection("Logging"));

            // Important step for using Memcached Cache or SQLite Cache
            //app.UseEasyCaching();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
