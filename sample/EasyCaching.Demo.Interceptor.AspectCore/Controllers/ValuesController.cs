namespace EasyCaching.Demo.Interceptor.AspectCore.Controllers
{
    using EasyCaching.Demo.Interceptor.AspectCore.Services;
    using Microsoft.AspNetCore.Mvc;

    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        private readonly IDateTimeService _service;

        public ValuesController(IDateTimeService service)
        {
            this._service = service;
        }

        [HttpGet]
        public string Get()
        {
            return _service.GetCurrentUtcTime();
        }
    }
}
