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
| EasyCaching.CSRedis  | ![](https://img.shields.io/nuget/v/EasyCaching.CSRedis.svg) | ![](https://img.shields.io/nuget/dt/EasyCaching.CSRedis.svg)

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
        //configuration
        services.AddEasyCaching(option=> 
        {
            //use memory cache that named default
            option.UseInMemory("default");

            //use redis cache that named redis1
            option.UseRedis(config => 
            {
                config.DBConfig.Endpoints.Add(new ServerEndPoint("127.0.0.1", 6379));
            }, "redis1")
            .WithMessagePack()//with messagepack serialization
            ;            
        });    
    }    
}
```

###  Step 3 : Write code in you controller 

```csharp
[Route("api/[controller]")]
public class ValuesController : Controller
{
    // //when using single provider
    // private readonly IEasyCachingProvider _provider;
    //when using multiple provider
    private readonly IEasyCachingProviderFactory _factory;

    public ValuesController(
        //IEasyCachingProvider provider, 
        IEasyCachingProviderFactory factory
        )
    {
        //this._provider = provider;
        this._factory = factory;
    }

    [HttpGet]
    public string Handle()
    {
        //var provider = _provider;
        //get the provider from factory with its name
        var provider = _factory.GetCachingProvider("redis1");    

        //Set
        provider.Set("demo", "123", TimeSpan.FromMinutes(1));
            
        //Set Async
        await provider.SetAsync("demo", "123", TimeSpan.FromMinutes(1));                  
    }
}
```

## Documentation

For more helpful information about EasyCaching, please click [here](http://easycaching.readthedocs.io/en/latest/) for EasyCaching's documentation. 

## Examples

See [sample](https://github.com/catcherwong/EasyCaching/tree/master/sample)

## Todo List

### Caching Providers

- [x] Memory
- [x] Redis(Based on [StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis))
- [x] Redis(Based on [csredis](https://github.com/2881099/csredis))
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

- [x] Redis (not release yet)
- [x] RabbitMQ (not release yet)

### Others

- [x] Configuration
- [x] Caching Region (one region with an instance of provider)
- [x] Caching Statistics
- [ ] UI Manager
- [x] Logger
- [ ] Caching Warm Up 
- [ ] ...

## Contributing

Pull requests, issues and commentary! 

## License
[![FOSSA Status](https://app.fossa.io/api/projects/git%2Bgithub.com%2Fdotnetcore%2FEasyCaching.svg?type=large)](https://app.fossa.io/projects/git%2Bgithub.com%2Fdotnetcore%2FEasyCaching?ref=badge_large)
