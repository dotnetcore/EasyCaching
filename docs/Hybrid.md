# HybridCachingProvider

HybridCachingProvider will combine local caching and distributed caching together.

# How to use ?

## 1. Install the packages via Nuget

```
Install-Package EasyCaching.HybridCache

Install-Package EasyCaching.InMemory

Install-Package EasyCaching.Redis
```

## 2. Config in Startup class

```csharp
public class Startup
{
    //...
    
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddMvc();
        
        services.AddDefaultInMemoryCacheForHybrid();
        services.AddDefaultRedisCacheForHybrid(option =>
        {
            option.Endpoints.Add(new ServerEndPoint("127.0.0.1", 6379));
            option.Password = "";
        });

        services.AddDefaultHybridCache();

        services.AddSingleton(factory =>
        {
            Func<string, IEasyCachingProvider> accesor = key =>
            {
                if(key.Equals(HybridCachingKeyType.LocalKey))
                {
                    return factory.GetService<DefaultInMemoryCachingProvider>();
                }
                else if(key.Equals(HybridCachingKeyType.DistributedKey))
                {
                    return factory.GetService<DefaultRedisCachingProvider>();
                }
                else
                {
                    throw new KeyNotFoundException();
                }
            };
            return accesor;
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

        //Refresh
        _provider.Refresh("key", "123", TimeSpan.FromMinutes(1));

        //Refresh Async
        await _provider.RefreshAsync("key", "123", TimeSpan.FromMinutes(1));
        
        //RemoveByPrefix
        _provider.RemoveByPrefix("prefix");
        
        //RemoveByPrefix async
        await _provider.RemoveByPrefixAsync("prefix");
        
         //SetAll
        _provider.SetAll(new Dictionary<string, string>()
        {
            {"key:1","value1"},
            {"key:2","value2"}
        }, TimeSpan.FromMinutes(1));

        //SetAllAsync
        await _provider.SetAllAsync(new Dictionary<string, string>()
        {
            {"key:1","value1"},
            {"key:2","value2"}
        }, TimeSpan.FromMinutes(1));

        //GetAll
        var res = _provider.GetAll(new List<string> { "key:1", "key:2" });

        //GetAllAsync
        var res = await _provider.GetAllAsync(new List<string> { "key:1", "key:2" });
        
        //GetByPrefix
        var res = _provider.GetByPrefix<T>("prefix");
        
        //GetByPrefixAsync
        var res = await _provider.GetByPrefixAsync<T>("prefix");
        
        //RemoveAll
        _provider.RemoveAll(new List<string> { "key:1", "key:2" });

        //RemoveAllAsync
        awiat _provider.RemoveAllAsync(new List<string> { "key:1", "key:2" });
    }
}
```
