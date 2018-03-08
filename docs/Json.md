# DefaultJsonSerializer

DefaultJsonSerializer is a serializer based on **Newtonsoft.Json**.

# How to Use?

## Install the package via Nuget

```
Install-Package EasyCaching.Serialization.Json
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
        //specify to use json serializer
        services.AddDefaultJsonSerializer();
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
        //specify the Transcoder use json serializer.
        services.AddDefaultJsonSerializer();
    }
}
```
