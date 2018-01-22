namespace EasyCaching.Demo.HybridCache
{
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using EasyCaching.Core.Internal;
    using EasyCaching.HybridCache;
    using EasyCaching.InMemory;
    using EasyCaching.Redis;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using EasyCaching.Core;
    using System.Collections.Generic;

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

            services.AddDefaultInMemoryCacheForHybrid();
            services.AddDefaultRedisCacheForHybrid(option =>
            {
                option.Endpoints.Add(new ServerEndPoint("127.0.0.1", 6379));
                option.Password = "";
            });

            services.AddDefaultHybridCache();

            services.AddSingleton(factory =>
            {
                Func<string, IEasyCachingProvider> accesor = key =>
                {
                    if(key.Equals(HybridCachingKeyType.LocalKey))
                    {
                        return factory.GetService<InMemoryCachingProvider>();
                    }
                    else if(key.Equals(HybridCachingKeyType.DistributedKey))
                    {
                        return factory.GetService<DefaultRedisCachingProvider>();
                    }
                    else
                    {
                        throw new KeyNotFoundException();
                    }
                };
                return accesor;
            });


        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
