namespace EasyCaching.Demo.InMemory.Controllers
{
    using EasyCaching.Core;
    using Microsoft.AspNetCore.Mvc;
    using System;

    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        private readonly IEasyCachingProvider _provider;

        public ValuesController(IEasyCachingProvider provider)
        {
            this._provider = provider;
        }

        // GET api/values?type=1
        [HttpGet]
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
    }
}
