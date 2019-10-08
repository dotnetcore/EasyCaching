namespace EasyCaching.Demo.Interceptors
{
    using AspectCore.Extensions.DependencyInjection;
    using Autofac.Extensions.DependencyInjection;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Hosting;

    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                // for aspcectcore
                .UseServiceProviderFactory(new AspectCoreServiceProviderFactory())
                //// for castle
                //.UseServiceProviderFactory(new AutofacServiceProviderFactory())
            ;
    }
}
