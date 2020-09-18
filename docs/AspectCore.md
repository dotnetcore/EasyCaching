# Caching Intercept via AspectCore

EasyCaching.Interceptor.AspectCore is a caching interceptor library which is based on **EasyCaching.Core** and **AspectCore**.

When using this library, it can help us to separate operations between business logic and caching logic.

# How to use ?

Before using **EasyCaching.Interceptor.AspectCore**, you should specify which type of caching you want to use!! In following example, we will use EasyCaching.InMemory.

## 1. Install the package via Nuget

```
Install-Package EasyCaching.Interceptor.AspectCore 

Install-Package EasyCaching.InMemory
```

## 2. Define services

Define interface first.

```csharp
public interface IDemoService
{
    [EasyCachingAble(Expiration = 10)]
    string GetCurrentUtcTime();

    [EasyCachingPut(CacheKeyPrefix = "AspectCore")]
    string PutSomething(string str);

    [EasyCachingEvict(IsBefore = true)]
    void DeleteSomething(int id);
}
```

Just implement the above interface.

```csharp
public class DemoService : IDemoService
{
    public void DeleteSomething(int id)
    {
        System.Console.WriteLine("Handle delete something..");
    }

    public string GetCurrentUtcTime()
    {
        return System.DateTime.UtcNow.ToString();
    }

    public string PutSomething(string str)
    {
        return str;
    }
}
```

## 3. Config in Startup class

There are some difference between .NET Core 2.x and .NET Core 3.1.

### .NET Core 3.1

Need to use EasyCaching above version 0.8.0

First, add the use of UseServiceProviderFactory to the Program.

```cs
// for aspcectcore
using AspectCore.Extensions.Hosting;
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
        ;
}
```

Second, you need to add the ConfigureContainer method to the Startup.

```cs
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
            options.UseInMemory("m1");
        });

        services.AddControllers();

        services.AddTransient<IDemoService, DemoService>();
        // AspectCore
        services.ConfigureAspectCoreInterceptor(options => options.CacheProviderName = "m1");
       
    }

     public void Configure(IApplicationBuilder app)
     {           
         app.UseRouting();
         app.UseEndpoints(endpoints =>
         {
            endpoints.MapControllers();
         });
     }
}
```

### .NET Core 2.x

EasyCaching above version 0.8.0 no longer supports .NET Core 2.x


```csharp
public class Startup
{
   // others...

    public IServiceProvider ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IDemoService, DemoService>();

        services.AddEasyCaching(option=> 
        {
            // use memory cache
            option.UseInMemory("default");
        });

        services.AddMvc();

        return services.ConfigureAspectCoreInterceptor(options =>
        {
            // Specify which provider you want to use
            options.CacheProviderName = "default";
        });
    } 
}
```

### 4. Call the service

The following code shows how to use in ASP.NET Core Web API.

```csharp
[Route("api/[controller]")]
public class ValuesController : Controller
{
    private readonly IDemoService _service;

    public ValuesController(IDemoService service)
    {
        this._service = service;
    }

    [HttpGet]
    public string Get()
    {
        if(type == 1)
        {
            return _service.GetCurrentUtcTime();
        }
        else if(type == 2)
        {
            _service.DeleteSomething(1);
            return "ok";
        }
        else if(type == 3)
        {
            return _service.PutSomething("123");
        }
        else
        {
            return "other";
        }
    }
}
```
