namespace EasyCaching.Demo.Interceptors
{
    using AspectCore.Extensions.DependencyInjection;
    using Autofac;
    using EasyCaching.Core;
    using EasyCaching.Demo.Interceptors.Services;
    using EasyCaching.Interceptor.AspectCore;
    using EasyCaching.Interceptor.Castle;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IAspectCoreService, AspectCoreService>();

            services.AddEasyCaching(options =>
            {
                options.UseInMemory();

                //options.UseRedis(config =>
                //{
                //    config.DBConfig = new RedisDBOptions { Configuration = "localhost" };
                //});

                //options.WithJson(config => { config.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.None; });
                //options.WithMessagePack();
                //options.WithProtobuf();
            });

            services.AddControllers();

            //1 AspectCore
            services.ConfigureAspectCoreInterceptor(options => options.CacheProviderName = EasyCachingConstValue.DefaultInMemoryName);

            services.AddTransient<ICastleService, CastleService>();

            //2 Castle  
            services.ConfigureCastleInterceptor(options => options.CacheProviderName = EasyCachingConstValue.DefaultInMemoryName);
        }

        #region ConfigureContainer should be only one
        //// ConfigureContainer is where you can register things directly
        //// with Autofac. This runs after ConfigureServices so the things
        //// here will override registrations made in ConfigureServices.
        //// Don't build the container; that gets done for you by the factory.
        // for castle
        //public void ConfigureContainer(ContainerBuilder builder)
        //{
        //    builder.ConfigureCastleInterceptor();
        //} 
        #endregion

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}