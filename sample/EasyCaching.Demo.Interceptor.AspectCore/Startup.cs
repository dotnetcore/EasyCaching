namespace EasyCaching.Demo.Interceptor.AspectCore
{
    using EasyCaching.Demo.Interceptor.AspectCore.Services;
    using EasyCaching.InMemory;
    using EasyCaching.Interceptor.AspectCore;
    using global::AspectCore.Configuration;
    using global::AspectCore.Injector;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
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

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IDateTimeService, DateTimeService>();

            services.AddDefaultInMemoryCache();

            services.AddMvc();

            //1. all default
            return services.ConfigureAspectCoreInterceptor();

            //2. default and customize
            //Action<IServiceContainer> action = x =>
            //{
            //    x.AddType<IDateTimeService, DateTimeService>();
            //};

            //return services.ConfigureAspectCoreInterceptor(action);

            //3. all customize
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
