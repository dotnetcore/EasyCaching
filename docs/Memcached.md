EasyCaching.Memcached is a memcached caching lib which is based on **EasyCaching.Core** and **[EnyimMemcachedCore](https://github.com/cnblogs/EnyimMemcachedCore)**.

When you use this lib , it means that you will handle the data of your memcached servers . As usual , we will use it as distributed caching .

# How to use ?

## Basic Usages

### 1. Install the package via Nuget

```
Install-Package EasyCaching.Memcached
```

### 2. Config in Startup class

```csharp
public class Startup
{
    //...
    
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddMvc();
        
        //Important step for Memcached Cache
        services.AddDefaultMemcached(option=>
        {                
            option.AddServer("127.0.0.1",11211);            
        });        
    }
    
    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
        //Important step for Memcache Cache
        app.UseDefaultMemcached();    
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

## Advanced Usages

As we all know , before we store data in redis database , the data need to be serialized !

**EnyimMemcachedCore** contains a default serializer that uses **System.Runtime.Serialization.Formatters.Binary** to handle (de)serialization . 

However , performance of the default serializer is not very well ! Many of us can choose another serializer to handle (de)serialization .

### 1.Install the serialization pack via Nuget

```
Install-Package EasyCaching.Serialization.MessagePack
```
 
### 2. Config in Startup class

```csharp
public class Startup
{
    //...
    
    public void ConfigureServices(IServiceCollection services)
    {
        //other services.

        //Important step for Redis Caching       
        services.AddDefaultMemcached(option=>
        {                
            option.AddServer("127.0.0.1",11211);
            //specify the Transcoder use messagepack .
            option.Transcoder = new MessagePackFormatterTranscoder(new DefaultMessagePackSerializer()) ;
        });
    }

    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
        //Important step for Memcache Cache
        app.UseDefaultMemcached();    
    }
}
```

### 3 Call Memcached Caching Provider

The same as the basic usage !
