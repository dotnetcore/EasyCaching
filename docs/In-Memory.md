# DefaultInMemoryCachingProvider

EasyCaching.InMemory is an in-memory caching lib which is based on **EasyCaching.Core**.

When you use this lib, it means that you will handle the memory of current server. As usual, we named it as local caching.

## How to use ?

### 1. Install the package via Nuget

```
Install-Package EasyCaching.InMemory
```

### 2. Config in Startup class

There are two way's how you can configure caching provider.

By C# code:

```csharp
public class Startup
{
    //...

    public void ConfigureServices(IServiceCollection services)
    {
        //other services.

        //Important step for In-Memory Caching
        services.AddEasyCaching(options =>
        {
            // use memory cache with a simple way
            options.UseInMemory("default");
            
            // use memory cache with your own configuration
            options.UseInMemory(config => 
            {
                config.DBConfig = new InMemoryCachingOptions
                {
                    // scan time, default value is 60s
                    ExpirationScanFrequency = 60, 
                    // total count of cache items, default value is 10000
                    SizeLimit = 100,       

                    // below two settings are added in v0.8.0
                    // enable deep clone when reading object from cache or not, default value is true.
                    EnableReadDeepClone = true,
                    // enable deep clone when writing object to cache or not, default valuee is false.
                    EnableWriteDeepClone = false,
                };
                // the max random second will be added to cache's expiration, default value is 120
                config.MaxRdSecond = 120;
                // whether enable logging, default is false
                config.EnableLogging = false;
                // mutex key's alive time(ms), default is 5000
                config.LockMs = 5000;
                // when mutex key alive, it will sleep some time, default is 300
                config.SleepMs = 300;
            }, "default1");
        });
    }
}
```

Alternatively you can store configuration in the `appsettings.json`.

```csharp
public class Startup
{
    //...

    public void ConfigureServices(IServiceCollection services)
    {
        //other services.

        //Important step for In-Memory Caching
        services.AddEasyCaching(options =>
        {
            //use memory cache
            options.UseInMemory(Configuration, "default", "easycaching:inmemory");
        });
    }
}
```

`appsettings.json` example:

```JSON
"easycaching": {
    "inmemory": {
        "MaxRdSecond": 120,
        "EnableLogging": false,
        "LockMs": 5000,
        "SleepMs": 300,
        "DBConfig":{
            "SizeLimit": 10000,
            "ExpirationScanFrequency": 60,
            "EnableReadDeepClone": true,
            "EnableWriteDeepClone": false
        }
    }
}
```

### 3. Call the EasyCachingProvider

Following code shows how to use EasyCachingProvider in ASP.NET Core Web API.

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

## Precautions

If you need to modify the data after you read from cache, don't forget the enable deep clone, otherwise, the cached data will be modified.

By the way, deep clone will hurt the performance, so if you don't need it, you should disable.
