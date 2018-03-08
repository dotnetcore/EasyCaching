# DefaultProtobufSerializer

DefaultProtobufSerializer is a serializer based on **protobuf-net**.

# How to Use?

## Install the package via Nuget

```
Install-Package EasyCaching.Serialization.Protobuf
```

## Use In EasyCaching.Redis

```
public class Startup
{
    //others...

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddMvc();

        services.AddDefaultRedisCache(option=>
        {                
            option.Endpoints.Add(new ServerEndPoint("127.0.0.1", 6379));
            option.Password = "";                                                  
        });   
        //specify to use protobuf serializer
        services.AddDefaultProtobufSerializer();
    }
}
```

## Use In EasyCaching.Memcached

```
public class Startup
{
    //others...

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddMvc();

        services.AddDefaultMemcached(op=>
        {                
            op.AddServer("127.0.0.1",11211);            
        });
        //specify the Transcoder use protobuf serializer.
        services.AddDefaultProtobufSerializer();
    }
}
```
