# DefaultInMemoryCachingProvider

EasyCaching.InMemory is a in-memory caching lib which is based on **EasyCaching.Core**.

When you use this lib , it means that you will handle the memory of current server . As usual , we named it as local caching .

## How to use ?

### 1. Install the package via Nuget

```
Install-Package EasyCaching.InMemory
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

        //Important step for In-Memory Caching
        services.AddEasyCaching(option =>
        {
            // use memory cache with a simple way
            option.UseInMemory("default");
            
            // use memory cache with your own configuration
            config.UseInMemory(options => 
            {
                options.DBConfig = new InMemoryCachingOptions
                {
                    // scan time, default value is 60s
                    ExpirationScanFrequency = 60, 
                    // total count of cache items, default value is 10000
                    SizeLimit = 100 
                };
                // the max random second will be added to cache's expiration, default value is 120
                options.MaxRdSecond = 120;
                // whether enable logging, default is false
                options.EnableLogging = false;
                // mutex key's alive time(ms), default is 5000
                options.LockMs = 5000;
                // when mutex key alive, it will sleep some time, default is 300
                options.SleepMs = 300;
            }, "default1");
        });
    }
}
```

What's more, we also can read the configuration from `appsettings.json`.

```csharp
public class Startup
{
    //...

    public void ConfigureServices(IServiceCollection services)
    {
        //other services.

        //Important step for In-Memory Caching
        services.AddEasyCaching(option =>
        {
            //use memory cache
            option.UseInMemory(Configuration, "default", "easycahing:inmemory");
        });
    }
}
```

And what we add in `appsettings.json` are as following:

```JSON
"easycaching": {
    "inmemory": {
        "MaxRdSecond": 120,
        "EnableLogging": false,
        "LockMs": 5000,
        "SleepMs": 300,
        "DBConfig":{
            "SizeLimit": 10000,
            "ExpirationScanFrequency": 60
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
    }
}
```