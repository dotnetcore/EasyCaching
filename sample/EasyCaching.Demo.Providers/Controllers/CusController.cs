namespace EasyCaching.Demo.Providers.Controllers
{
    using EasyCaching.Core;
    using Microsoft.AspNetCore.Mvc;
    using System;

    [Route("api/[controller]")]
    public class CusController : Controller
    {
        private readonly IEasyCachingProviderFactory _factory;

        public CusController(IEasyCachingProviderFactory factory)
        {
            this._factory = factory;
        }

        // GET api/cus/inmem?name=Default
        [HttpGet]
        [Route("inmem")]
        public string Get(string name = EasyCachingConstValue.DefaultInMemoryName)
        {
            var provider = _factory.GetCachingProvider(name);
            var val = name.Equals("cus") ? "cus" : "default";
            var res = provider.Get("demo", () => val, TimeSpan.FromMinutes(1));
            return $"cached value : {res}";               
        }

        // GET api/cus/redis?name=Default
        [HttpGet]
        [Route("redis")]
        public string GetRedis(string name = EasyCachingConstValue.DefaultRedisName)
        {
            var provider = _factory.GetCachingProvider(name);
            var val = name.Equals("redis1") ? $"redis1-{Guid.NewGuid()}" : $"redis2-{Guid.NewGuid()}";
            var res = provider.Get("named-provider", () => val, TimeSpan.FromMinutes(1));
            return $"cached value : {res}";
        }


        // GET api/cus/com?name=Default
        [HttpGet]
        [Route("com")]
        public string GetCom(string name = "cus")
        {
            var provider = _factory.GetCachingProvider(name);
            var val = $"{name}-{Guid.NewGuid()}";
            var res = provider.Get("named-provider2", () => val, TimeSpan.FromMinutes(1));
            return $"cached value : {res}";
        }
    }
}
