namespace EasyCaching.Demo.Redis
{
    using EasyCaching.Core.Internal;
    using EasyCaching.Redis;
    using EasyCaching.Serialization.MessagePack;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    
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

            //services.AddDefaultMessagePackSerializer();
            services.AddDefaultRedisCache(option=>
            {                
                option.Endpoints.Add(new ServerEndPoint("127.0.0.1", 6379));
                option.Password = "";                                                  
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
