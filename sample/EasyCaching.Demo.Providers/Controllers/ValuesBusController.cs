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
        [Route("get2")]
        public async Task<string> Get2()
        {
            var rd = new Random(1000);
            for (int i = 0; i < 5; i++)
             {
                var val = rd.Next().ToString();
                await _provider.SetAsync($"demo{i}", val, TimeSpan.FromSeconds(5000));
                var provider = _factory.GetCachingProvider("cus");
                var v1 = provider.Get<string>($"demow{i}");
                //Console.WriteLine($"{i}-->{v1}");

                await _provider.SetAsync($"demow{i}", $"changeda-{val}", TimeSpan.FromSeconds(5000));

                //var v2 = provider.Get<string>($"demo{i}");
                //Console.WriteLine($"after--{i}-->{v2}");
                //Console.WriteLine("------------------");
            }
            return $"hybrid";
        }

     
    }
}
