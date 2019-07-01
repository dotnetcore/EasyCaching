# EasyCachingProviderFactory

`EasyCachingProviderFactory` is a new feature which was implemented in v0.4.0. It provides a factory to get providers 
that we register in startup.

# Why ?

Sometimes we may have multiple instances of one or more caching types, but there are not hybrid caching, 
just some separate caching instances based on different businesses.

After releasing v0.4.0 of EasyCaching, we can deal with this scenario.

# How to use ?

This usage of `EasyCachingProviderFactory` is similar with `HttpClientFactory`.

There are two types of providers(`IEasyCachingProvider` and `IRedisCachingProvider`) that `EasyCachingProviderFactory` can create.

Following examples uses two InMemory caching provders and two Redis caching providers.

## 1. Install the packages via Nuget

```
Install-Package EasyCaching.InMemory

Install-Package EasyCaching.Redis
```

## 2. Config in Startup class

```csharp
public void ConfigureServices(IServiceCollection services)  
{  
    //other ..  

    services.AddEasyCaching(option=> 
    {
        //use memory cache
        option.UseInMemory("inmemory1");

        //use memory cache
        option.UseInMemory("inmemory2");

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
  
    // GET api/values/inmem1
    [HttpGet]  
    [Route("inmem1")]  
    public string GetInMemory()  
    {  
        var provider = _factory.GetCachingProvider("inmemory1");  
        var val = $"memory1-{Guid.NewGuid()}";  
        var res = provider.Get("named-provider", () => val, TimeSpan.FromMinutes(1));  
        Console.WriteLine($"Type=InMemory,Key=named-provider,Value={res},Time:{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");  
        return $"cached value : {res}";                 
    }  
    
    // GET api/values/inmem2
    [HttpGet]  
    [Route("inmem2")]  
    public string GetInMemory()  
    {  
        var provider = _factory.GetCachingProvider("inmemory2");  
        var val = $"memory2-{Guid.NewGuid()}";  
        var res = provider.Get("named-provider", () => val, TimeSpan.FromMinutes(1));  
        Console.WriteLine($"Type=InMemory,Key=named-provider,Value={res},Time:{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");  
        return $"cached value : {res}";                 
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
