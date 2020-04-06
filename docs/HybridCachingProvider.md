# HybridCachingProvider

HybridCachingProvider can be created by `IHybridProviderFactory`.

# How to Use?

## 1. Install the packages via Nuget

```
Install-Package EasyCaching.HybridCache
Install-Package EasyCaching.InMemory
Install-Package EasyCaching.Redis
Install-Package EasyCaching.Bus.Redis
```

## 2. Config in Startup class

```csharp
public void ConfigureServices(IServiceCollection services)  
{  
    //other ..  

    services.AddEasyCaching(options =>
    {
        // local
        options.UseInMemory("m1");
        // distributed
        options.UseRedis(config =>
        {
            config.DBConfig.Endpoints.Add(new ServerEndPoint("127.0.0.1", 6379));
            config.DBConfig.Database = 5;
        }, "myredis");

        // combine local and distributed
        options.UseHybrid(config =>
        {
            config.TopicName = "test-topic";
            config.EnableLogging = true;

            // specify the local cache provider name after v0.5.4
            config.LocalCacheProviderName = "m1";
            // specify the distributed cache provider name after v0.5.4
            config.DistributedCacheProviderName = "myredis";
        }, "h1")
        // use redis bus
        .WithRedisBus(busConf =>
        {
            busConf.Endpoints.Add(new ServerEndPoint("127.0.0.1", 6379));
        });
    });
}  
```

### 3. Call the IHybridCachingProvider

Following code shows how to use IHybridCachingProvider in ASP.NET Core Web API.

```csharp
[Route("api/[controller]")]
[ApiController]
public class ValuesController : ControllerBase
{
    private readonly IHybridCachingProvider _hybrid;

    public ValuesController(IHybridProviderFactory hybridFactory)
    {
        this._hybrid = hybridFactory.GetHybridCachingProvider("h1");
    }

    // GET api/values
    [HttpGet]
    public ActionResult<IEnumerable<string>> Get()
    {
        var res = _hybrid.Get<string>("cacheKey");

        return new string[] { "value1", "value2", res.Value };
    }

    // GET api/values/set
    [HttpGet("set")]
    public ActionResult<string> Set()
    {
        // the same key for different value of 
        _hybrid.Set("cacheKey", "val-from app1", TimeSpan.FromMinutes(1));

        return "ok";
    }
}
```
