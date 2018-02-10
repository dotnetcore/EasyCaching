![](https://raw.githubusercontent.com/catcherwong/EasyCaching/master/media/easycaching-icon.png)

EasyCaching is a open source caching library that contains basic usages and some advanced usages of caching which can help us to handle caching more easier!

[![Coverage Status](https://coveralls.io/repos/github/catcherwong/EasyCaching/badge.svg?branch=master)](https://coveralls.io/github/catcherwong/EasyCaching?branch=master)

[![GitHub license](https://img.shields.io/github/license/catcherwong/EasyCaching.svg)](https://github.com/catcherwong/EasyCaching/blob/master/LICENSE)

## CI Build Status

| Platform | Build Server | Status  |
|--------- |------------- |---------|
| AppVeyor |  Windows |[![Build status](https://ci.appveyor.com/api/projects/status/ji7513h4uv4ysq2i?svg=true)](https://ci.appveyor.com/project/catcherwong/easycaching) |
| Travis   | Linux/OSX | [![Build Status](https://travis-ci.org/catcherwong/EasyCaching.svg?branch=master)](https://travis-ci.org/catcherwong/EasyCaching) |    

## Nuget Packages

| Package Name |  Version | Downloads
|--------------|  ------- | ----
| EasyCaching.Core | ![](https://img.shields.io/nuget/v/EasyCaching.Core.svg) | ![](https://img.shields.io/nuget/dt/EasyCaching.Core.svg)
| EasyCaching.InMemory | ![](https://img.shields.io/nuget/v/EasyCaching.InMemory.svg) | ![](https://img.shields.io/nuget/dt/EasyCaching.InMemory.svg)
| EasyCaching.Redis | ![](https://img.shields.io/nuget/v/EasyCaching.Redis.svg) | ![](https://img.shields.io/nuget/dt/EasyCaching.Redis.svg)
| EasyCaching.Memcached | ![](https://img.shields.io/nuget/v/EasyCaching.Memcached.svg) | ![](https://img.shields.io/nuget/dt/EasyCaching.Memcached.svg)
| EasyCaching.SQLite | ![](https://img.shields.io/nuget/v/EasyCaching.SQLite.svg) | ![](https://img.shields.io/nuget/dt/EasyCaching.SQLite.svg)
| EasyCaching.Serialization.MessagePack | ![](https://img.shields.io/nuget/v/EasyCaching.Serialization.MessagePack.svg) | ![](https://img.shields.io/nuget/dt/EasyCaching.Serialization.MessagePack.svg)
| EasyCaching.Interceptor.Castle | ![](https://img.shields.io/nuget/v/EasyCaching.Interceptor.Castle.svg) | ![](https://img.shields.io/nuget/dt/EasyCaching.Interceptor.Castle.svg)
| EasyCaching.Interceptor.AspectCore | ![](https://img.shields.io/nuget/v/EasyCaching.Interceptor.AspectCore.svg) | ![](https://img.shields.io/nuget/dt/EasyCaching.Interceptor.AspectCore.svg)
| EasyCaching.HybridCache  | ![](https://img.shields.io/nuget/v/EasyCaching.HybridCache.svg) | ![](https://img.shields.io/nuget/dt/EasyCaching.HybridCache.svg)

## Basci Usages 

### #1 Caching APIs usage

#### Step 1 : Install the package

Choose one kinds of caching type that you needs and install it via Nuget.

```
Install-Package EasyCaching.InMemory
Install-Package EasyCaching.Redis
Install-Package EasyCaching.SQLite
Install-Package EasyCaching.Memcached
```

#### Step 2 : Config in your Startup class

Different types of caching hvae their own way to config.

Here are samples show you how to config.

```csharp
public class Startup
{
    //...
    
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddMvc();

        //1. In-Memory Cache
        services.AddDefaultInMemoryCache();
        
        //2. Redis Cache
        //services.AddDefaultRedisCache(option=>
        //{                
        //    option.Endpoints.Add(new ServerEndPoint("127.0.0.1", 6379));
        //    option.Password = "";
        //});
        
        //3. Memcached Cache
        //services.AddDefaultMemcached(option=>
        //{                
        //    option.AddServer("127.0.0.1",11211);
        //    //specify the Transcoder use messagepack .
        //    option.Transcoder = new MessagePackFormatterTranscoder(new DefaultMessagePackSerializer()) ;
        //});
        
        //4. SQLite Cache
        //services.AddSQLiteCache(option=>{});
    }
    
    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
        //3. Memcache Cache
        //app.UseDefaultMemcached();
    
        //4. SQLite Cache
        //app.UseSQLiteCache();
    }
}
```

####  Step 3 : Write code in you controller 

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
        //Set
        _provider.Set("demo", "123", TimeSpan.FromMinutes(1));
            
        //Set Async
        await _provider.SetAsync("demo", "123", TimeSpan.FromMinutes(1));   
        
        //Get without data retriever
        var res = _provider.Get<string>("demo");
        
        //Get without data retriever Async
        var res = await _provider.GetAsync<string>("demo");
        
        //Get
        var res = _provider.Get("demo", () => "456", TimeSpan.FromMinutes(1));
        
        //Get Async    
        var res = await _provider.GetAsync("demo",async () => await Task.FromResult("456"), TimeSpan.FromMinutes(1));   
                
        //Remove
        _provider.Remove("demo");

        //Remove Async
        await _provider.RemoveAsync("demo");
           
        //Refresh
        _provider.Refresh("demo", "123", TimeSpan.FromMinutes(1));

        //Refresh Async
        await _provider.RefreshAsync("demo", "123", TimeSpan.FromMinutes(1));               
    }
}
```

### #2 Caching Serializer

Serializer is mainly building for distributed caching . 

Redis Caching has implemented a default serializer that uses **System.Runtime.Serialization.Formatters.Binary** to handle serialization and deserialization .

Memcahced Caching is based on [EnyimMemcachedCore](https://github.com/cnblogs/EnyimMemcachedCore) , it also has a default serializer named **BinaryFormatterTranscoder** .

[EasyCaching.Serialization.MessagePack]() is an extension package providing MessagePack serialization for distributed caches

How to use ?

#### Step 1 : Install packages via Nuget

```
Install-Package EasyCaching.Serialization.MessagePack
```

#### Step 2 : Config in Startup class 

```csharp

public class Startup
{
    //...
    
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddMvc();
        //1. For Redis
        services.AddDefaultRedisCache(option=>
        {                
            option.Endpoints.Add(new ServerEndPoint("127.0.0.1", 6379));
            option.Password = "";       
        });
        services.AddDefaultMessagePackSerializer();
        //2. For Memcached
        //services.AddDefaultMemcached(op=>
        //{                
        //    op.AddServer("127.0.0.1",11211);
        //
        //    op.Transcoder = new MessagePackFormatterTranscoder(new DefaultMessagePackSerializer()) ;
        //});
        //services.AddDefaultMessagePackSerializer();
    }
}
```

### #3 Others

coming soon !

## Advanced Usages

### #1 Caching Interceptor

EasyCaching contains three attributes(`EasyCachingAble`,`EasyCachingPut`,`EasyCachingEvict`) to handle interceptor. 

- EasyCachingAble : triggers cache population
- EasyCachingPut : updates the cache without interfering with the method execution
- EasyCachingEvict : triggers cache eviction

And EasyCaching provide two implementations of caching interceptor, one is based on Castle, the other one is based on AspectCore.

The following example use Castle to show how to config.

#### Step 1 : Define you service class

```csharp
public interface IDateTimeService 
{        
    string GetCurrentUtcTime();
}

//IEasyCaching is important for this!!
public class DateTimeService : IDateTimeService ,  IEasyCaching
{        
    [EasyCachingInterceptor(Expiration = 10)]
    public string GetCurrentUtcTime()
    {
        return System.DateTime.UtcNow.ToString();
    }

    [EasyCachingEvict(CacheKeyPrefix = "CastleExample")]
    public string EvictTest()
    {
        return "EvictTest";
    }

    [EasyCachingPut(CacheKeyPrefix = "CastleExample")]
    public string PutTest(int num)
    {
        return $"PutTest-{num}";
    }
}
```

#### Step 2 : Config in Startup class 

```csharp
public class Startup
{
    //...

    public IServiceProvider ConfigureServices(IServiceCollection services)
    {
        //..

        //service 
        services.AddTransient<IDateTimeService,DateTimeService>();

        //Choose one of caching provider that you want to use.
        services.AddDefaultInMemoryCache();

        //CastleInterceptor
        return services.ConfigureCastleInterceptor();
    }
}
```

#### Step 3 : Write code in you controller 

```csharp
[Route("api/[controller]")]
public class ValuesController : Controller
{
    private readonly IDateTimeService _service;

    public ValuesController(IDateTimeService service)
    {
        this._service = service;
    }

    [HttpGet]
    public string Get()
    {
        _service.GetCurrentUtcTime();
        _service.EvictTest();
        _service.PutTest();
        return "";
    }
}
```

### #3 Hybrid Caching

Hybrid Caching is mainly building for combine local caching and distributed caching. It is called 2-tiers caching.

#### Step 1 : Install the package

```
Install-Package EasyCaching.HybridCache
```

#### Step 2 : Config in startup class

```csharp
public void ConfigureServices(IServiceCollection services)
{
    //...
    //1. Add local caching
    services.AddDefaultInMemoryCacheForHybrid();
    //2. Add distributed caching
    services.AddDefaultRedisCacheForHybrid(option =>
    {
        option.Endpoints.Add(new ServerEndPoint("127.0.0.1", 6379));
        option.Password = "";
    });
    //3. Add hybrid caching
    services.AddDefaultHybridCache();
    //4. Important step for different impls of only one interface .
    services.AddSingleton(factory =>
    {
        Func<string, IEasyCachingProvider> accesor = key =>
        {
            if(key.Equals(HybridCachingKeyType.LocalKey))
            {
                return factory.GetService<InMemoryCachingProvider>();
            }
            else if(key.Equals(HybridCachingKeyType.DistributedKey))
            {
                return factory.GetService<DefaultRedisCachingProvider>();
            }
            else
            {
                throw new KeyNotFoundException();
            }
        };
        return accesor;
    });
}
```

#### Step 2: Calling the HybridCachingProvider

```csharp
[Route("api/[controller]")]
public class ValuesController : Controller
{
    private readonly IHybridCachingProvider _provider;

    public ValuesController(IHybridCachingProvider provider)
    {
        this._provider = provider;
    }
    
    [HttpGet]    
    public string Get()
    {
        _provider.xxx();
    }
}
```

### #4 Others

coming soon !

## Examples

See [sample](https://github.com/catcherwong/EasyCaching/tree/master/sample)

## Todo List

### Caching Providers

- [x] Memory
- [x] Redis
- [x] SQLite
- [x] Memcached
- [x] Hybrid(Combine local caching and distributed caching)
- [ ] Others...

### Basic Caching API

- [x] Get/GetAsync(with data retriever)
- [x] Get/GetAsync(without data retriever)
- [x] Set/SetAsync
- [x] Remove/RemoveAsync
- [x] Refresh/RefreshAsync
- [x] RemoveByPrefix/RemoveByPrefixAsync
- [ ] Flush/FlushAsync(whether is in need ? )
- [ ] Others...

### Serializer Extensions 

- [x] BinaryFormatter
- [x] MessagePack
- [ ] Json
- [ ] Others...

### Caching Interceptor

Not support Hybird Caching provider yet .

- AspectCore
    1. [x] EasyCachingAble
    2. [x] EasyCachingPut
    3. [x] EasyCachingEvict
- Castle
    1. [x] EasyCachingAble
    2. [x] EasyCachingPut
    3. [x] EasyCachingEvict
- Others ..
    

### Others

- [ ] Documents(Writing..)
- [ ] Configuration
- [ ] Bus
- [ ] Caching Region
- [ ] Caching Statistics
- [ ] UI Manager
- [ ] Logger
- [ ] Caching Warm Up 
- [ ] ...


