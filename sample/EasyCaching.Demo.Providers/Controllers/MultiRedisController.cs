namespace EasyCaching.Demo.Providers.Controllers
{
    using EasyCaching.Core;
    using Microsoft.AspNetCore.Mvc;

    [Route("api/mredis")]
    public class MultiRedisController : Controller
    {
        private readonly IRedisCachingProvider _redis1;
        private readonly IRedisCachingProvider _redis2;

        public MultiRedisController(IEasyCachingProviderFactory factory)
        {
            this._redis1 = factory.GetRedisProvider("redis1");
            this._redis2 = factory.GetRedisProvider("redis2");
        }

        // GET api/mredis
        [HttpGet]
        public string Get()
        {
            _redis1.StringSet("keyredis1", "val");

            var res1 = _redis1.StringGet("keyredis1");
            var res2 = _redis2.StringGet("keyredis1");

            return $"redis1 cached value: {res1}, redis2 cached value : {res2}";               
        }             
    }
}
