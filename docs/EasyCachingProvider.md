# EasyCachingProvider

EasyCachingProvider can be created by `IEasyCachingProviderFactory`.

# How to Use?

## 1. Install the packages via Nuget

```
Install-Package EasyCaching.InMemory
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
}  
```
