# DefaultEtcdCachingProvider

Etcd is a hybrid memory and disk Kv Store, so it can support much larger data storage than memory.

EasyCaching.Etcd is a lib that is based on **EasyCaching.Core** and **dotnet-etcd-core**.


# How to use ?

## 1. Install the package via Nuget

```
Install-Package EasyCaching.Etcd
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
            //use Etcd cache
            option.UseEtcd(config =>
            {
                config.Address = "http://127.0.0.1:2379";
                config.Timeout = 30000;
                // Etcd must be set SerializerName
                config.SerializerName = "msg";
            })
            .WithMessagePack("msg");
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
