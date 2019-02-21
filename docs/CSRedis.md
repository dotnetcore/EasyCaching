# DefaultCSRedisCachingProvider

EasyCaching.CSRedis is a redis caching lib which is based on **EasyCaching.Core** and **CSRedisCore**.

When you use this lib , it means that you will handle the data of your redis servers . As usual , we will use it as distributed caching .

# How to use ?

## Basic Usages

### 1. Install the package via Nuget

```
Install-Package EasyCaching.CSRedis
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
        services.AddEasyCaching(option =>
        {
            option.UseCSRedis(config =>
            {
                config.DBConfig = new CSRedisDBOptions
                {
                    ConnectionStrings = new System.Collections.Generic.List<string>
                    {
                        "127.0.0.1:6388,defaultDatabase=13,poolsize=10"
                    }
                };
            });
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
        services.AddEasyCaching(option =>
        {
            option.UseCSRedis(Configuration, "myredisname", "easycaching:csredis");
        });
    }
}
```

And what we add in `appsettings.json` are as following:

```JSON
"easycaching": {
    "csredis": {
        "CachingProviderType": 2,
        "MaxRdSecond": 120,
        "Order": 2,
        "dbconfig": {
            "ConnectionStrings":[
                "127.0.0.1:6388,defaultDatabase=13,poolsize=10"
            ]
        }
    }
}
```

### 3. Call the IEasyCachingProvider

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
            
        //others ...
    }
}
```

### 4. Redis Feature Provider

Redis has many other data types, such as Hash, List .etc.

EasyCaching.CSRedis also support those types that named redis feature provider.

If you want to use this feature provider, just call `IRedisCachingProvider` to replace `IEasyCachingProvider` .


```csharp
[Route("api/[controller]")]
public class ValuesController : Controller
{
    private readonly IRedisCachingProvider _provider;

    public ValuesController(IRedisCachingProvider provider)
    {
        this._provider = provider;
    }

    [HttpGet]
    public string Get()
    {
        // HMSet
        var res = _provider.HMSet(cacheKey, new Dictionary<string, string>
        {
            {"a1","v1"},{"a2","v2"}
        });
            
        //others ...
    }
}
```
