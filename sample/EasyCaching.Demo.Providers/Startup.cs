namespace EasyCaching.Demo.Providers
{
    using EasyCaching.Core;
    using EasyCaching.Core.Internal;
    using EasyCaching.InMemory;
    using EasyCaching.HybridCache;
    using EasyCaching.Memcached;
    using EasyCaching.Redis;
    using EasyCaching.SQLite;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
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
            services.AddMvc();

            //1. Important step for using InMemory Cache
            services.AddDefaultInMemoryCache();

            ////2. Important step for using Memcached Cache
            //services.AddDefaultMemcached(op =>
            //{
            //    op.AddServer("127.0.0.1", 11211);
            //});

            ////3. Important step for using Redis Cache
            //services.AddDefaultRedisCache(option =>
            //{
            //    option.Endpoints.Add(new ServerEndPoint("127.0.0.1", 6379));
            //    option.Password = "";
            //});

            ////4. Important step for using SQLite Cache
            //services.AddSQLiteCache(option => { });

            ////5. Important step for using Hybrid Cache
            ////5.1. Local Cache
            //services.AddDefaultInMemoryCache(x=>
            //{
            //    x.Order = 1;
            //});
            ////5.2 Distributed Cache
            //services.AddDefaultRedisCache(option =>
            //{
            //    option.Endpoints.Add(new ServerEndPoint("127.0.0.1", 6379));
            //    option.Password = "";
            //}, x=>
            //{
            //    x.Order = 2;//this value should greater than local caching provider
            //});
            ////5.3 Hybrid
            //services.AddDefaultHybridCache();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env,ILoggerFactory loggerFactory)
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
