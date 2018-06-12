# DefaultRedisCachingProvider

EasyCaching.Redis is a redis caching lib which is based on **EasyCaching.Core** and **StackExchange.Redis**.

When you use this lib , it means that you will handle the data of your redis servers . As usual , we will use it as distributed caching .

# How to use ?

## Basic Usages

### 1. Install the package via Nuget

```
Install-Package EasyCaching.Redis
```

### 2. Config in Startup class

There are two options you can choose when you config the caching provider.

First of all, we can config by C# code.

```csharp
public class Startup
{
    //...
    
    public void ConfigureServices(IServiceCollection services)
    {
        //other services.

        //Important step for Redis Caching       
        services.AddDefaultRedisCache(option=>
        {                
            option.DBConfig.Endpoints.Add(new ServerEndPoint("127.0.0.1", 6379));
            option.DBConfig.Password = "";
        });
    }
}
```

What's more, we also can read the configuration from `appsettings.json`.

```cs
public class Startup
{
    //...
    
    public void ConfigureServices(IServiceCollection services)
    {
        //other services.

        //Important step for Redis Caching
        services.AddDefaultRedisCache(Configuration); 
    }
}
```

And what we add in `appsettings.json` are as following:

```JSON
"easycaching": {
    "redis": {
        "CachingProviderType": 2,
        "MaxRdSecond": 120,
        "Order": 2,
        "dbconfig": {
            "Password": null,
            "IsSsl": false,
            "SslHost": null,
            "ConnectionTimeout": 5000,
            "AllowAdmin": true,
            "Endpoints": [
                {
                    "Host": "localhost",
                    "Port": 6739
                }
            ],
            "Database": 0
        }
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
    }
}
```
