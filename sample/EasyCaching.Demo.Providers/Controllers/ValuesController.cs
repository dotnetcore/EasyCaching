namespace EasyCaching.Demo.Providers.Controllers
{
    using EasyCaching.Core;
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Threading.Tasks;

    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        //1. InMemory,Memcached,Redis,SQLite,FasterKv
        private readonly IEasyCachingProvider _provider;

        public ValuesController(IEasyCachingProvider provider)
        {
            this._provider = provider;
        }

        ////2. Hybird Cache
        //private readonly IHybridCachingProvider _provider;

        //public ValuesController(IHybridCachingProvider provider)
        //{
        //    this._provider = provider;
        //}

        // GET api/values/get?type=1
        [HttpGet]
        [Route("get")]
        public string Get(string str)
        {
            var method = str.ToLower();
            switch(method)
            {
                case "get" :
                    var res = _provider.Get("demo", () => "456", TimeSpan.FromMinutes(1));
                    return $"cached value : {res}";
                case "set" :
                    _provider.Set("demo", "123", TimeSpan.FromMinutes(1));
                    return "seted";
                case "remove" :
                    _provider.Remove("demo");
                    return "removed";
                case "getcount" :
                    var count = _provider.GetCount();
                    return $"{count}";
                default :
                    return "default";                    
            }
        }


        // GET api/values/getasync?type=1
        [HttpGet]
        [Route("getasync")]
        public async Task<string> GetAsync(string str)
        {
            var method = str.ToLower();
            switch (method)
            {
                case "get":
                    var res = await _provider.GetAsync("demo", async () => await Task.FromResult("456"), TimeSpan.FromMinutes(1));
                    return $"cached value : {res}";
                case "set":
                    await _provider.SetAsync("demo", "123", TimeSpan.FromMinutes(1));
                    return "seted";
                case "remove":
                    await _provider.RemoveAsync("demo");
                    return "removed";
                case "getcount":
                    var count = _provider.GetCount();
                    return $"{count}";
                default:
                    return "default";
            }
        }

        // GET api/values/stats
        [HttpGet]
        [Route("stats")]
        public string Stats()
        {
            var hit = _provider.CacheStats.GetStatistic(StatsType.Hit);
            var missed = _provider.CacheStats.GetStatistic(StatsType.Missed);

            return $"hit={hit},missed={missed}";
        }
    }
}
