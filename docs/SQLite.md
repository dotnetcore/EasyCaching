# DefaultSQLiteCachingProvider

SQLite is another choice of local caching. 

There are two scenarios where SQLite is recommended to use as our local caching!

1. Persisted caching item.
2. Rebuild caching item.

EasyCaching.SQLite contains most of caching operations that we can use easily.

EasyCaching.SQLite is a lib that is based on **EasyCaching.Core** and **Microsoft.Data.SQLite**.


# How to use ?

## 1. Install the package via Nuget

```
Install-Package EasyCaching.SQLite
```

## 2. Config in Startup class

```csharp
public class Startup
{
    //...
    
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddMvc();
        
        //Important step for SQLite Cache
        services.AddEasyCaching(option => 
        {
            //use sqlite cache
            option.UseSQLite(config =>
            {
                config.DBConfig = new SQLiteDBOptions { FileName = "my.db" };
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
        
        //RemoveByPrefix
        _provider.RemoveByPrefix("prefix");
        
        //RemoveByPrefix async
        await _provider.RemoveByPrefixAsync("prefix");
    }
}
```
