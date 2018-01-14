namespace EasyCaching.Demo.Interceptor.AspectCore
{
    using System;
    using EasyCaching.Demo.Interceptor.AspectCore.Services;
    using EasyCaching.InMemory;
    using EasyCaching.Interceptor.AspectCore;
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

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IDateTimeService, DateTimeService>();

            services.AddDefaultInMemoryCache();

            services.AddMvc();

            return services.ConfigureAspectCoreInterceptor();
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
