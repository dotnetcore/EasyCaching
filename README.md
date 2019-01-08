![](media/easycaching-icon.png?raw=true)

EasyCaching is an open source caching library that contains basic usages and some advanced usages of caching which can help us to handle caching more easier!

[![Coverage Status](https://coveralls.io/repos/github/catcherwong/EasyCaching/badge.svg?branch=master)](https://coveralls.io/github/catcherwong/EasyCaching?branch=master)
[![Member project of .NET Core Community](https://img.shields.io/badge/member%20project%20of-NCC-9e20c9.svg)](https://github.com/dotnetcore)
[![GitHub license](https://img.shields.io/github/license/dotnetcore/EasyCaching.svg)](https://github.com/dotnetcore/EasyCaching/blob/master/LICENSE)
[![FOSSA Status](https://app.fossa.io/api/projects/git%2Bgithub.com%2Fdotnetcore%2FEasyCaching.svg?type=shield)](https://app.fossa.io/projects/git%2Bgithub.com%2Fdotnetcore%2FEasyCaching?ref=badge_shield)

## CI Build Status

| Platform | Build Server | Master Status  | Dev Status  |
|--------- |------------- |---------|---------|
| AppVeyor |  Windows/Linux |[![Build status](https://ci.appveyor.com/api/projects/status/4x6qal9c1r10wn6x/branch/master?svg=true)](https://ci.appveyor.com/project/catcherwong/easycaching-48okb/branch/master) |[![Build status](https://ci.appveyor.com/api/projects/status/4x6qal9c1r10wn6x/branch/dev?svg=true)](https://ci.appveyor.com/project/catcherwong/easycaching-48okb/branch/dev)|
| Travis   | Linux/OSX | [![Build Status](https://travis-ci.org/dotnetcore/EasyCaching.svg?branch=master)](https://travis-ci.org/dotnetcore/EasyCaching) |    [![Build Status](https://travis-ci.org/dotnetcore/EasyCaching.svg?branch=dev)](https://travis-ci.org/dotnetcore/EasyCaching) |

## Nuget Packages

### Core

| Package Name |  Version | Downloads
|--------------|  ------- | ----
| EasyCaching.Core | ![](https://img.shields.io/nuget/v/EasyCaching.Core.svg) | ![](https://img.shields.io/nuget/dt/EasyCaching.Core.svg)

### Provider

| Package Name |  Version | Downloads
|--------------|  ------- | ----
| EasyCaching.InMemory | ![](https://img.shields.io/nuget/v/EasyCaching.InMemory.svg) | ![](https://img.shields.io/nuget/dt/EasyCaching.InMemory.svg)
| EasyCaching.Redis | ![](https://img.shields.io/nuget/v/EasyCaching.Redis.svg) | ![](https://img.shields.io/nuget/dt/EasyCaching.Redis.svg)
| EasyCaching.Memcached | ![](https://img.shields.io/nuget/v/EasyCaching.Memcached.svg) | ![](https://img.shields.io/nuget/dt/EasyCaching.Memcached.svg)
| EasyCaching.SQLite | ![](https://img.shields.io/nuget/v/EasyCaching.SQLite.svg) | ![](https://img.shields.io/nuget/dt/EasyCaching.SQLite.svg)
| EasyCaching.HybridCache  | ![](https://img.shields.io/nuget/v/EasyCaching.HybridCache.svg) | ![](https://img.shields.io/nuget/dt/EasyCaching.HybridCache.svg)

### Interceptor

| Package Name |  Version | Downloads
|--------------|  ------- | ----
| EasyCaching.Interceptor.Castle | ![](https://img.shields.io/nuget/v/EasyCaching.Interceptor.Castle.svg) | ![](https://img.shields.io/nuget/dt/EasyCaching.Interceptor.Castle.svg)
| EasyCaching.Interceptor.AspectCore | ![](https://img.shields.io/nuget/v/EasyCaching.Interceptor.AspectCore.svg) | ![](https://img.shields.io/nuget/dt/EasyCaching.Interceptor.AspectCore.svg)

### Serializer

| Package Name |  Version | Downloads
|--------------|  ------- | ----
| EasyCaching.Serialization.MessagePack | ![](https://img.shields.io/nuget/v/EasyCaching.Serialization.MessagePack.svg) | ![](https://img.shields.io/nuget/dt/EasyCaching.Serialization.MessagePack.svg)
| EasyCaching.Serialization.Json | ![](https://img.shields.io/nuget/v/EasyCaching.Serialization.Json.svg) | ![](https://img.shields.io/nuget/dt/EasyCaching.Serialization.Json.svg)
| EasyCaching.Serialization.Protobuf | ![](https://img.shields.io/nuget/v/EasyCaching.Serialization.Protobuf.svg) | ![](https://img.shields.io/nuget/dt/EasyCaching.Serialization.Protobuf.svg)

### Others

| Package Name |  Version | Downloads
|--------------|  ------- | ----
| EasyCaching.ResponseCaching | ![](https://img.shields.io/nuget/v/EasyCaching.ResponseCaching.svg) | ![](https://img.shields.io/nuget/dt/EasyCaching.ResponseCaching.svg)

## Basic Usages 

### Step 1 : Install the package

Choose one kinds of caching type that you needs and install it via Nuget.

```
Install-Package EasyCaching.InMemory
Install-Package EasyCaching.Redis
Install-Package EasyCaching.SQLite
Install-Package EasyCaching.Memcached
```

### Step 2 : Config in your Startup class

Different types of caching hvae their own way to config.

Here is a sample show you how to config.

```csharp
public class Startup
{
    //...
    
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddMvc();

        //In-Memory Cache
        services.AddDefaultInMemoryCache();
        
        //Read from appsetting.json
        //services.AddDefaultInMemoryCache(Configuration);               
    }    
}
```

###  Step 3 : Write code in you controller 

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
                
        //others ....
    }
}
```

## Advanced Usages

### Multiple instances for single provider

After v0.4.0, EasyCaching import `IEasyCachingProviderFactory` to create providers by users.

Configure in `Startup.cs` at first.

```cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddDefaultRedisCacheWithFactory("redis1",option =>
    {
        option.DBConfig.Endpoints.Add(new ServerEndPoint("127.0.0.1", 6379));
    });

    services.AddDefaultRedisCacheWithFactory("redis2", option =>
    {
        option.DBConfig.Endpoints.Add(new ServerEndPoint("127.0.0.1", 6380));
    });
}
```
Use `IEasyCachingProviderFactory` to create the provider.

```cs
private readonly IEasyCachingProviderFactory _factory;

public CusController(IEasyCachingProviderFactory factory)
{
    this._factory = factory;
}

[HttpGet]
[Route("")]
public string GetRedis()
{
    //use 6379
    var provider1 = _factory.GetCachingProvider("redis1");    
    var val1 = provider1.Get("named-provider-1", () => "redis1", TimeSpan.FromMinutes(1));
    
    //use 6380
    var provider2 = _factory.GetCachingProvider("redis2");    
    var val2 = provider2.Get("named-provider-2", () => "redis2", TimeSpan.FromMinutes(1));
    
    return $"OK";
}
```

## Documentation

For more helpful information about EasyCaching, please click [here](http://easycaching.readthedocs.io/en/latest/) for EasyCaching's documentation. 

## Examples

See [sample](https://github.com/catcherwong/EasyCaching/tree/master/sample)

## Todo List

### Caching Providers

- [x] Memory
- [x] Redis
- [x] SQLite
- [x] Memcached
- [x] Hybrid(Combine local caching and distributed caching)
- [ ] Disk
- [ ] Others...

### Basic Caching API

- [x] Get/GetAsync(with data retriever)
- [x] Get/GetAsync(without data retriever)
- [x] Set/SetAsync
- [x] Remove/RemoveAsync
- [x] Refresh/RefreshAsync
- [x] RemoveByPrefix/RemoveByPrefixAsync
- [x] SetAll/SetAllAsync
- [x] GetAll/GetAllAsync
- [x] GetByPrefix/GetByPrefixAsync
- [x] RemoveAll/RemoveAllAsync
- [x] GetCount
- [x] Flush/FlushAsync
- [x] TrySet/TrySetAsync
- [ ] Others...

### Serializer Extensions 

- [x] BinaryFormatter
- [x] MessagePack
- [x] Json
- [x] ProtoBuf
- [ ] Others...

### Caching Interceptor

- [x] AspectCore
- [x] Castle
- [ ] Others ..
    
1. EasyCachingAble
2. EasyCachingPut
3. EasyCachingEvict

> Note: Not support Hybird Caching provider yet.

### Caching Bus

- [ ] Redis
- [ ] RabbitMQ

### Others

- [x] Configuration
- [ ] Caching Region
- [x] Caching Statistics
- [ ] UI Manager
- [x] Logger
- [ ] Caching Warm Up 
- [ ] ...

## Contributing

Pull requests, issues and commentary! 

## License
[![FOSSA Status](https://app.fossa.io/api/projects/git%2Bgithub.com%2Fdotnetcore%2FEasyCaching.svg?type=large)](https://app.fossa.io/projects/git%2Bgithub.com%2Fdotnetcore%2FEasyCaching?ref=badge_large)
