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

        // GET api/cus/get?name=Default
        [HttpGet]
        [Route("get")]
        public string Get(string name = "Default")
        {
            var provider = _factory.GetCachingProvider(name);
            var val = name.Equals("cus") ? "cus" : "default";
            var res = provider.Get("demo", () => val, TimeSpan.FromMinutes(1));
            return $"cached value : {res}";               
        }
    }
}
