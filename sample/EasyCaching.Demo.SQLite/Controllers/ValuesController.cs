namespace EasyCaching.Demo.SQLite.Controllers
{
    using EasyCaching.Core;
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Threading.Tasks;

    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        private readonly IEasyCachingProvider _provider;

        public ValuesController(IEasyCachingProvider provider)
        {
            this._provider = provider;
        }

        // GET api/values/get?type=1
        [HttpGet]
        [Route("get")]
        public string Get(int type = 1)
        {
            if (type == 1)
            {
                _provider.Remove("demo");
                return "removed";
            }
            else if (type == 2)
            {
                _provider.Set("demo", "123", TimeSpan.FromMinutes(1));
                return "seted";
            }
            else if (type == 3)
            {
                var res = _provider.Get("demo", () => "456", TimeSpan.FromMinutes(1));
                return $"cached value : {res}";
            }
            else
            {
                return "error";
            }
        }


        // GET api/values/getasync?type=1
        [HttpGet]
        [Route("getasync")]
        public async Task<string> GetAsync(int type = 1)
        {
            if (type == 1)
            {
                await _provider.RemoveAsync("demo");
                return "removed";
            }
            else if (type == 2)
            {
                await _provider.SetAsync("demo", "123", TimeSpan.FromMinutes(1));
                return "seted";
            }
            else if (type == 3)
            {
                var res = await _provider.GetAsync("demo", async () => await Task.FromResult("456"), TimeSpan.FromMinutes(1));
                return $"cached value : {res}";
            }
            else
            {
                return "error";
            }
        }
    }
}
