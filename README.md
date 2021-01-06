![](media/easycaching-icon.png?raw=true)

EasyCaching is an open source caching library that contains basic usages and some advanced usages of caching which can help us to handle caching more easily!

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

| Package Name |  Version | Downloads
|--------------|  ------- | ----
| EasyCaching.Core | ![](https://img.shields.io/nuget/v/EasyCaching.Core.svg) | ![](https://img.shields.io/nuget/dt/EasyCaching.Core.svg)
| EasyCaching.InMemory | ![](https://img.shields.io/nuget/v/EasyCaching.InMemory.svg) | ![](https://img.shields.io/nuget/dt/EasyCaching.InMemory.svg)
| EasyCaching.Redis | ![](https://img.shields.io/nuget/v/EasyCaching.Redis.svg) | ![](https://img.shields.io/nuget/dt/EasyCaching.Redis.svg)
| EasyCaching.Memcached | ![](https://img.shields.io/nuget/v/EasyCaching.Memcached.svg) | ![](https://img.shields.io/nuget/dt/EasyCaching.Memcached.svg)
| EasyCaching.SQLite | ![](https://img.shields.io/nuget/v/EasyCaching.SQLite.svg) | ![](https://img.shields.io/nuget/dt/EasyCaching.SQLite.svg)
| EasyCaching.HybridCache  | ![](https://img.shields.io/nuget/v/EasyCaching.HybridCache.svg) | ![](https://img.shields.io/nuget/dt/EasyCaching.HybridCache.svg)
| EasyCaching.CSRedis  | ![](https://img.shields.io/nuget/v/EasyCaching.CSRedis.svg) | ![](https://img.shields.io/nuget/dt/EasyCaching.CSRedis.svg)
| EasyCaching.Interceptor.Castle | ![](https://img.shields.io/nuget/v/EasyCaching.Interceptor.Castle.svg) | ![](https://img.shields.io/nuget/dt/EasyCaching.Interceptor.Castle.svg)
| EasyCaching.Interceptor.AspectCore | ![](https://img.shields.io/nuget/v/EasyCaching.Interceptor.AspectCore.svg) | ![](https://img.shields.io/nuget/dt/EasyCaching.Interceptor.AspectCore.svg)
| EasyCaching.Serialization.MessagePack | ![](https://img.shields.io/nuget/v/EasyCaching.Serialization.MessagePack.svg) | ![](https://img.shields.io/nuget/dt/EasyCaching.Serialization.MessagePack.svg)
| EasyCaching.Serialization.Json | ![](https://img.shields.io/nuget/v/EasyCaching.Serialization.Json.svg) | ![](https://img.shields.io/nuget/dt/EasyCaching.Serialization.Json.svg)
| EasyCaching.Serialization.Protobuf | ![](https://img.shields.io/nuget/v/EasyCaching.Serialization.Protobuf.svg) | ![](https://img.shields.io/nuget/dt/EasyCaching.Serialization.Protobuf.svg)
| EasyCaching.Bus.RabbitMQ | ![](https://img.shields.io/nuget/v/EasyCaching.Bus.RabbitMQ.svg) | ![](https://img.shields.io/nuget/dt/EasyCaching.Bus.RabbitMQ.svg)
| EasyCaching.Bus.Redis | ![](https://img.shields.io/nuget/v/EasyCaching.Bus.Redis.svg) | ![](https://img.shields.io/nuget/dt/EasyCaching.Bus.Redis.svg)
| EasyCaching.Bus.CSRedis | ![](https://img.shields.io/nuget/v/EasyCaching.Bus.CSRedis.svg) | ![](https://img.shields.io/nuget/dt/EasyCaching.Bus.CSRedis.svg)
| EasyCaching.ResponseCaching | ![](https://img.shields.io/nuget/v/EasyCaching.ResponseCaching.svg) | ![](https://img.shields.io/nuget/dt/EasyCaching.ResponseCaching.svg)
| EasyCaching.Disk | ![](https://img.shields.io/nuget/v/EasyCaching.Disk.svg) | ![](https://img.shields.io/nuget/dt/EasyCaching.Disk.svg)
| EasyCaching.LiteDB | ![](https://img.shields.io/nuget/v/EasyCaching.LiteDB.svg) | ![](https://img.shields.io/nuget/dt/EasyCaching.LiteDB.svg)
| EasyCaching.Serialization.SystemTextJson | ![](https://img.shields.io/nuget/v/EasyCaching.Serialization.SystemTextJson.svg) | ![](https://img.shields.io/nuget/dt/EasyCaching.Serialization.SystemTextJson.svg)

## Basic Usages 

### Step 1 : Install the package

Choose caching provider that you need and install it via Nuget.

```
Install-Package EasyCaching.InMemory
Install-Package EasyCaching.Redis
Install-Package EasyCaching.SQLite
Install-Package EasyCaching.Memcached
```

### Step 2 : Configure Startup class

Each caching provider has it's own configuration options.

Here is a sample configuration for InMemory and Redis caching provider.

```csharp
public class Startup
{
    //...
    
    public void ConfigureServices(IServiceCollection services)
    {
        //configuration
        services.AddEasyCaching(options => 
        {
            //use memory cache that named default
            options.UseInMemory("default");

            // // use memory cache with your own configuration
            // options.UseInMemory(config => 
            // {
            //     config.DBConfig = new InMemoryCachingOptions
            //     {
            //         // scan time, default value is 60s
            //         ExpirationScanFrequency = 60, 
            //         // total count of cache items, default value is 10000
            //         SizeLimit = 100 
            //     };
            //     // the max random second will be added to cache's expiration, default value is 120
            //     config.MaxRdSecond = 120;
            //     // whether enable logging, default is false
            //     config.EnableLogging = false;
            //     // mutex key's alive time(ms), default is 5000
            //     config.LockMs = 5000;
            //     // when mutex key alive, it will sleep some time, default is 300
            //     config.SleepMs = 300;
            // }, "m2");

            //use redis cache that named redis1
            options.UseRedis(config => 
            {
                config.DBConfig.Endpoints.Add(new ServerEndPoint("127.0.0.1", 6379));
            }, "redis1")
            .WithMessagePack()//with messagepack serialization
            ;            
        });    
    }    
}
```

###  Step 3 : Write code in your controller 

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

Detailed EasyCaching documentation can be found [here](http://easycaching.readthedocs.io/en/latest/).

## Extension Libs

- [EasyCaching.Extensions](https://github.com/yrinleung/EasyCaching.Extensions)

- [EasyCaching.Extensions.EasyCompressor](https://github.com/mjebrahimi/EasyCompressor/blob/master/src/EasyCaching.Extensions.EasyCompressor/README.md)

## Examples

See [sample](https://github.com/catcherwong/EasyCaching/tree/master/sample)

## Todo List

See [ToDo List](docs/ToDoList.md)

## Contributing

Pull requests, issues and commentary! 

Also can join our QQ group.

![](media/qqgroup.jpg?raw=true)

## License
[![FOSSA Status](https://app.fossa.io/api/projects/git%2Bgithub.com%2Fdotnetcore%2FEasyCaching.svg?type=large)](https://app.fossa.io/projects/git%2Bgithub.com%2Fdotnetcore%2FEasyCaching?ref=badge_large)
