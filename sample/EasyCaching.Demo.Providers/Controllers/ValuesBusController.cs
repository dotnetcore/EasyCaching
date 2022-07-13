namespace EasyCaching.Demo.Providers.Controllers
{
    using EasyCaching.Core;
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Threading.Tasks;

    [Route("api/[controller]")]
    public class ValuesBusController : Controller
    {
        //2. Hybird Cache
        private readonly IHybridCachingProvider _provider;
        private readonly IEasyCachingProviderFactory _factory;

        public ValuesBusController(IHybridCachingProvider provider, IEasyCachingProviderFactory factory)
        {
            this._provider = provider;
            _factory = factory;
        }

        // GET api/values
        [HttpGet]
        [Route("")]
        public string Get2()
        {
            _provider.Set("demo2", "val", TimeSpan.FromSeconds(5000));
            var provider = _factory.GetCachingProvider("cus");
            var v1 = provider.Get<string>("demo2");

            _provider.Set("demo2", "changeda", TimeSpan.FromSeconds(5000));

            var v2 = provider.Get<string>("demo2");
            return $"hybrid";
        }

     
    }
}
