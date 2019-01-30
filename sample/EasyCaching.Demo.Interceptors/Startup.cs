namespace EasyCaching.Demo.Interceptors
{
    using EasyCaching.Core;
    using EasyCaching.Demo.Interceptors.Services;
    using EasyCaching.Interceptor.AspectCore;
    using EasyCaching.Interceptor.Castle;
    using EasyCaching.InMemory;
    using EasyCaching.Redis;
    using EasyCaching.Serialization.MessagePack;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using System;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        //1.AspectCore
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IAspectCoreService, AspectCoreService>();

            services.AddEasyCaching(options=> 
            {
                options.UseInMemory();

                //options.UseRedis(config =>
                //{
                //    config.DBConfig = new RedisDBOptions { Configuration = "localhost" };
                //});
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            //1.1. all default
            return services.ConfigureAspectCoreInterceptor();

            //1.2. default and customize
            //Action<IServiceContainer> action = x =>
            //{
            //    x.AddType<IAspectCoreService, AspectCoreService>();
            //};

            //return services.ConfigureAspectCoreInterceptor(action);

            //1.3. all customize
            //Action<IServiceContainer> action = x =>
            //{
            //    x.AddType<IDateTimeService, DateTimeService>();
            //    x.Configure(config =>
            //    {
            //        config.Interceptors.AddTyped<EasyCachingInterceptor>(method => typeof(Core.Internal.IEasyCaching).IsAssignableFrom(method.DeclaringType));
            //    });
            //};

            //return services.ConfigureAspectCoreInterceptor(action, true);
        }

        ////2. Castle
        //public IServiceProvider ConfigureServices(IServiceCollection services)
        //{
        //    services.AddTransient<ICastleService, CastleService>();

        //    services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

        //    services.AddEasyCaching(options =>
        //    {
        //        options.UseInMemory();

        //        //options.UseRedis(config =>
        //        //{
        //        //    config.DBConfig = new RedisDBOptions { Configuration = "localhost" };
        //        //});
        //    });

        //    //2.1. all default
        //    return services.ConfigureCastleInterceptor();

        //    //2.2. default and customize
        //    //Action<ContainerBuilder> action = x =>
        //    //{
        //    //    x.RegisterType<CastleService>().As<ICastleService>();
        //    //};

        //    //return services.ConfigureCastleInterceptor(action);

        //    //2.3. all customize
        //    //Action<ContainerBuilder> action = x =>
        //    //{
        //    //    x.RegisterType<DateTimeService>().As<IDateTimeService>();

        //    //    var assembly = Assembly.GetExecutingAssembly();
        //    //    x.RegisterType<EasyCachingInterceptor>();

        //    //    x.RegisterAssemblyTypes(assembly)
        //    //        .Where(type => typeof(Core.Internal.IEasyCaching).IsAssignableFrom(type) && !type.GetTypeInfo().IsAbstract)
        //    //        .AsImplementedInterfaces()
        //    //        .InstancePerLifetimeScope()
        //    //        .EnableInterfaceInterceptors()
        //    //        .InterceptedBy(typeof(EasyCachingInterceptor));
        //    //};

        //    //return services.ConfigureCastleInterceptor(action, true);
        //}

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
