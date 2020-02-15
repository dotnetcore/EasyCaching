namespace EasyCaching.Demo.Interceptors
{
    using AspectCore.Extensions.DependencyInjection;
    using AspectCore.Extensions.Hosting;
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
                 .UseServiceContext()
            //// for castle
            //.UseServiceProviderFactory(new AutofacServiceProviderFactory())
            ;
    }
}
