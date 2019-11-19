# DefaultMemcachedCachingProvider

EasyCaching.Memcached is a memcached caching lib which is based on **EasyCaching.Core** and **[EnyimMemcachedCore](https://github.com/cnblogs/EnyimMemcachedCore)**.

When you use this lib, it means that you will handle the data of your memcached servers. As usual, we will use it as distributed caching.

# How to use ?

## Basic Usages

### 1. Install the package via Nuget

```
Install-Package EasyCaching.Memcached
```

### 2. Config in Startup class

```csharp
public class Startup
{
    //...

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddMvc();

        //Important step for Memcached Cache
        services.AddEasyCaching(option => 
        {
            //use memmemcachedory cache
            option.UseMemcached(config => 
            {
                config.DBConfig.AddServer("127.0.0.1", 11211);
            });
        });
    }
}
```

### 3. Call the EasyCachingProvider

The following code show how to use EasyCachingProvider in ASP.NET Core Web API.

```csharp
[Route("api/[controller]")]
public class ValuesController : Controller
{
    private readonly IEasyCachingProvider _provider;

    public ValuesController(IEasyCachingProvider provider)
    {
        this._provider = provider;
    }

    [HttpGet]
    public string Get()
    {
        //Remove
        _provider.Remove("demo");
        
        //Set
        _provider.Set("demo", "123", TimeSpan.FromMinutes(1));
            
        //Get
        var res = _provider.Get("demo", () => "456", TimeSpan.FromMinutes(1));
        
        //Get without data retriever
        var res = _provider.Get<string>("demo");
        
        //Remove Async
        await _provider.RemoveAsync("demo");
           
        //Set Async
        await _provider.SetAsync("demo", "123", TimeSpan.FromMinutes(1));   
            
        //Get Async    
        var res = await _provider.GetAsync("demo",async () => await Task.FromResult("456"), TimeSpan.FromMinutes(1));   
        
        //Get without data retriever Async
        var res = await _provider.GetAsync<string>("demo");
    }
}
```
