# DefaultInMemoryCachingProvider

EasyCaching.InMemory is a in-memory caching lib which is based on **EasyCaching.Core** and **Microsoft.Extensions.Caching.Memory**.

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
            //use memory cache
            option.UseInMemory("default");
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
        "CachingProviderType": 1,
        "MaxRdSecond": 120,
        "Order": 2,
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
