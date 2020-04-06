# HybridCachingProvider

HybridCachingProvider will combine local caching and distributed caching together.

The most important problem that this caching provider solves is that it keeps the newest local cached value.

When we modify a cached value, the provider will send a message to `EasyCaching Bus` so that it can notify other Apps to remove the old value.

The following image shows how it runs.

![](https://raw.githubusercontent.com/dotnetcore/EasyCaching/master/media/hybrid_details.png)

# How to use ?

## 1. Install the packages via Nuget

```
Install-Package EasyCaching.HybridCache
Install-Package EasyCaching.InMemory
Install-Package EasyCaching.Redis
Install-Package EasyCaching.Bus.Redis
```

## 2. Config in Startup class

```csharp
public class Startup
{
    //...

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddMvc();

        services.AddEasyCaching(option =>
        {
            // local
            option.UseInMemory("m1");
            // distributed
            option.UseRedis(config =>
            {
                config.DBConfig.Endpoints.Add(new Core.Configurations.ServerEndPoint("127.0.0.1", 6379));
                config.DBConfig.Database = 5;
            }, "myredis");

            // combine local and distributed
            option.UseHybrid(config =>
            {
                config.TopicName = "test-topic";
                config.EnableLogging = false;

                // specify the local cache provider name after v0.5.4
                config.LocalCacheProviderName = "m1";
                // specify the distributed cache provider name after v0.5.4
                config.DistributedCacheProviderName = "myredis";
            })
            // use redis bus
            .WithRedisBus(busConf => 
            {
                busConf.Endpoints.Add(new ServerEndPoint("127.0.0.1", 6380));
            });
        });
    }
}
```

### 3. Call `IHybridCachingProvider`

Following code shows how to use EasyCachingProvider in ASP.NET Core Web API.

```csharp
[Route("api/[controller]")]
public class ValuesController : Controller
{
    private readonly IHybridCachingProvider _provider;

    public ValuesController(IHybridCachingProvider provider)
    {
        this._provider = provider;
    }

    [HttpGet]
    public string Get()
    {
        //Set
        _provider.Set("demo", "123", TimeSpan.FromMinutes(1));

        //others
        //...
    }
}
```
