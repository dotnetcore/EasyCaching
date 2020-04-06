# RedisCachingProvider

RedisCachingProvider can be created by `IEasyCachingProviderFactory`.

# How to Use?

## 1. Install the packages via Nuget

```
Install-Package EasyCaching.Redis
```

## 2. Config in Startup class

```csharp
public void ConfigureServices(IServiceCollection services)  
{  
    //other ..  

    services.AddEasyCaching(option=> 
    {
          //use redis cache
        option.UseRedis(config => 
        {
            config.DBConfig.Endpoints.Add(new ServerEndPoint("127.0.0.1", 6379));
        }, "redis1");

        //use redis cache
        option.UseRedis(config => 
        {
            config.DBConfig.Endpoints.Add(new ServerEndPoint("127.0.0.1", 6380));
        }, "redis2");
    });
}  
```

### 3. Call the IEasyCachingProviderFactory

Following code shows how to use IEasyCachingProviderFactory in ASP.NET Core Web API.

```csharp
[Route("api/[controller]")]  
public class ValuesController : Controller  
{  
    private readonly IEasyCachingProviderFactory _factory;  
  
    public ValuesController(IEasyCachingProviderFactory factory)  
    {  
        this._factory = factory;  
    }  
  
    // GET api/values/redis1  
    [HttpGet]  
    [Route("redis1")]  
    public string GetRedis1()  
    {  
        var provider = _factory.GetCachingProvider("redis1");  
        var val =  $"redis1-{Guid.NewGuid()}";  
        var res = provider.Get("named-provider", () => val, TimeSpan.FromMinutes(1));  
        Console.WriteLine($"Type=redis1,Key=named-provider,Value={res},Time:{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");  
        return $"cached value : {res}";  
    }  
      
    // GET api/values/redis2  
    [HttpGet]  
    [Route("redis2")]  
    public string GetRedis2()  
    {  
        var provider = _factory.GetCachingProvider("redis2");  
        var val =  $"redis2-{Guid.NewGuid()}";  
        var res = provider.Get("named-provider", () => val, TimeSpan.FromMinutes(1));  
        Console.WriteLine($"Type=redis2,Key=named-provider,Value={res},Time:{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");  
        return $"cached value : {res}";  
    }  

    // GET api/values/redis3 
    [HttpGet]  
    [Route("redis3")]  
    public string GetRedis3()  
    {  
        var redis1 = factory.GetRedisProvider("redis1");
        var redis2 = factory.GetRedisProvider("redis2");

         _redis1.StringSet("keyredis1", "val");

        var res1 = _redis1.StringGet("keyredis1");
        var res2 = _redis2.StringGet("keyredis1");

        return $"redis1 cached value: {res1}, redis2 cached value : {res2}";  
    }  
}  
```
