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
        services.AddEasyCaching(option =>
        {
            option.UseRedis(config => 
            {
                config.DBConfig.Endpoints.Add(new ServerEndPoint("127.0.0.1", 6379));
            }, "redis1");
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
        services.AddEasyCaching(option =>
        {
            option.UseRedis(Configuration, "myredisname");
        });
    }
}
```

And what we add in `appsettings.json` are as following:

```JSON
"easycaching": {
    "redis": {
        "MaxRdSecond": 120,
        "EnableLogging": false,
        "LockMs": 5000,
        "SleepMs": 300,
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

EasyCaching.Redis also support those types that named redis feature provider.

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
