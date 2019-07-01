# Caching Intercept via AspectCore

EasyCaching.Interceptor.AspectCore is a caching interceptor library which is based on **EasyCaching.Core** and **AspectCore**.

When using this library, it can help us to separate operations between business logic and caching logic.

# How to use ?

Before using **EasyCaching.Interceptor.AspectCore**, you should specify which type of caching you want to use!! In following example, we will use EasyCaching.InMemory.

## 1. Install the package via Nuget

```
Install-Package EasyCaching.Interceptor.AspectCore 

Install-Package EasyCaching.InMemory
```

## 2. Define services

### 2.1 Define the interface

We need to add `EasyCachingAble`,`EasyCachingPut` or `EasyCachingEvict` on the methods where we want to simplify the caching operations.

The following list shows you which attributes you can use for caching:  

- EasyCachingAble , Read from cached items
- EasyCachingPut , Update the cached item
- EasyCachingEvict , Remove one cached item or multi cached items

These properties can be applied on attributes

Property | Description | Apply
---|---|---
CacheKeyPrefix | To specify the prefix of your cache key | All
CacheProviderName | To specify which provider you want to use | All
IsHighAvailability | Whether caching opreation will break your method | All
Expiration | To specify the expiration of your cache item，the unit is second | EasyCachingAble and EasyCachingPut
IsAll | Whether remove all the cached items start with the CacheKeyPrefix | EasyCachingEvict only
IsBefore | Remove the cached item before method excute or after method excute | EasyCachingEvict only

how to use example:

Define interface first.

```csharp
public interface IDemoService
{
    [EasyCachingAble(Expiration = 10)]
    string GetCurrentUtcTime();

    [EasyCachingPut(CacheKeyPrefix = "AspectCore")]
    string PutSomething(string str);

    [EasyCachingEvict(IsBefore = true)]
    void DeleteSomething(int id);
}
```

Just implement the above interface.

```csharp
public class DemoService : IDemoService
{
    public void DeleteSomething(int id)
    {
        System.Console.WriteLine("Handle delete something..");
    }

    public string GetCurrentUtcTime()
    {
        return System.DateTime.UtcNow.ToString();
    }

    public string PutSomething(string str)
    {
        return str;
    }
}
```

## 3. Config in Startup class

```csharp
public class Startup
{
   // others...

    public IServiceProvider ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IDemoService, DemoService>();

        services.AddEasyCaching(option=> 
        {
            // use memory cache
            option.UseInMemory("default");
        });

        services.AddMvc();

        return services.ConfigureAspectCoreInterceptor(options =>
        {
            // Specify which provider you want to use
            options.CacheProviderName = "default";
        });
    } 
}
```

### 3. Call the service

The following code shows how to use in ASP.NET Core Web API.

```csharp
[Route("api/[controller]")]
public class ValuesController : Controller
{
    private readonly IDemoService _service;

    public ValuesController(IDemoService service)
    {
        this._service = service;
    }

    [HttpGet]
    public string Get()
    {
        if(type == 1)
        {
            return _service.GetCurrentUtcTime();
        }
        else if(type == 2)
        {
            _service.DeleteSomething(1);
            return "ok";
        }
        else if(type == 3)
        {
            return _service.PutSomething("123");
        }
        else
        {
            return "other";
        }
    }
}
```
