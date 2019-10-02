namespace EasyCaching.Demo.ResponseCaching
{
    using EasyCaching.Core;
    using EasyCaching.InMemory;
    using EasyCaching.ResponseCaching;
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
            services.AddControllersWithViews();

            services.AddEasyCaching(x => { x.UseInMemory(); });
            services.AddEasyCachingResponseCaching(EasyCachingConstValue.DefaultInMemoryName);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseEasyCachingResponseCaching();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}