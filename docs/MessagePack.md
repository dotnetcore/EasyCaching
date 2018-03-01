# DefaultMessagePackSerializer

DefaultMessagePackSerializer is is a serializer based on **MessagePack**.

# How to Use?

## Install the package via Nuget

```
Install-Package EasyCaching.Serialization.MessagePack
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
        //put after AddDefaultRedisCache
        //in order to replace DefaultBinaryFormatterSerializer
        services.AddDefaultMessagePackSerializer();
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
            //specify the Transcoder use messagepack .
            op.Transcoder = "EasyCaching.Memcached.FormatterTranscoder,EasyCaching.Memcached" ;
            op.SerializationType = "EasyCaching.Serialization.MessagePack.DefaultMessagePackSerializer,EasyCaching.Serialization.MessagePack";
        });
    }
}
```
