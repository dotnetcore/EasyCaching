namespace EasyCaching.Demo.Providers
{
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
    using System.IO;

    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
             Host.CreateDefaultBuilder(args)
               .UseContentRoot(Directory.GetCurrentDirectory())
                    .ConfigureAppConfiguration((hosting, config) =>
                    {
                        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                            .AddJsonFile($"appsettings.{hosting.HostingEnvironment.EnvironmentName}.json", optional: true,
                                true);

                        config.AddEnvironmentVariables();
                    })
                 .ConfigureWebHostDefaults(webBuilder =>
                 {
                     webBuilder.UseStartup<Startup>();
                 })
             //.UseServiceProviderFactory(new AutofacServiceProviderFactory())
             ;
    }
}
