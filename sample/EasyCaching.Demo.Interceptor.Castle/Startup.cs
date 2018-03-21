namespace EasyCaching.Demo.Interceptor.Castle
{
    using Autofac;
    using Autofac.Extras.DynamicProxy;
    using EasyCaching.InMemory;
    using EasyCaching.Interceptor.Castle;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Reflection;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddTransient<IDateTimeService,DateTimeService>();

            services.AddDefaultInMemoryCache();

            //1. all default
            return services.ConfigureCastleInterceptor();

            //2. default and customize
            //Action<ContainerBuilder> action = x =>
            //{
            //    x.RegisterType<DateTimeService>().As<IDateTimeService>();
            //};

            //return services.ConfigureCastleInterceptor(action);

            //3. all customize
            //Action<ContainerBuilder> action = x =>
            //{
            //    x.RegisterType<DateTimeService>().As<IDateTimeService>();

            //    var assembly = Assembly.GetExecutingAssembly();
            //    x.RegisterType<EasyCachingInterceptor>();

            //    x.RegisterAssemblyTypes(assembly)
            //        .Where(type => typeof(Core.Internal.IEasyCaching).IsAssignableFrom(type) && !type.GetTypeInfo().IsAbstract)
            //        .AsImplementedInterfaces()
            //        .InstancePerLifetimeScope()
            //        .EnableInterfaceInterceptors()
            //        .InterceptedBy(typeof(EasyCachingInterceptor));
            //};

            //return services.ConfigureCastleInterceptor(action, true);
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
