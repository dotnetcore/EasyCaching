# Response Caching 

Response Caching is a feature which is mainly use for HTTP Cache.

**EasyCaching.ResponseCaching** is based on **Microsoft.AspNetCore.ResponseCaching**. And you can find more information via  **[Response Caching Middleware in ASP.NET Core
](https://docs.microsoft.com/en-us/aspnet/core/performance/caching/middleware?view=aspnetcore-2.0)** .

# How to Use?

## Install the package via Nuget

```
Install-Package EasyCaching.ResponseCaching
```

## Configuration

```
public class Startup
{
    //others...

    public void ConfigureServices(IServiceCollection services)
    {
        //add response caching
        services.AddEasyCachingResponseCaching();
        //which type of caching that you want to use
        services.AddEasyCaching(option=> 
        {            
            // option.Usexxx(...);
        });
    }
    
    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
        //use response caching
        app.UseEasyCachingResponseCaching();
    }
}
```

## ResponseCacheAttribute

```
[ResponseCache(Duration = 20)]
public IActionResult Index()
{
    return View(new Models.TestModel
    {
        LastUpdated = DateTimeOffset.UtcNow.ToString()
    });
}

[ResponseCache(Duration = 30, VaryByQueryKeys = new string[] { "page" })]
public IActionResult List(int page = 0)
{
    return Content(page.ToString());
}
```
