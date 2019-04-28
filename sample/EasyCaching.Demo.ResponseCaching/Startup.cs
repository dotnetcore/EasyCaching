using EasyCaching.Core;

namespace EasyCaching.Demo.ResponseCaching
{
    using EasyCaching.InMemory;
    using EasyCaching.ResponseCaching;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddEasyCaching(x => { x.UseInMemory(); });
            services.AddEasyCachingResponseCaching(EasyCachingConstValue.DefaultInMemoryName);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseEasyCachingResponseCaching();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}