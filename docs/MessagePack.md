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
        //specify to use messagepack serializer
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
        });
        //specify the Transcoder use messagepack serializer.
        services.AddDefaultMessagePackSerializer();
    }
}
```
