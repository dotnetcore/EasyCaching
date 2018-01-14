using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace EasyCaching.Demo.Interceptor.Castle.Controllers
{
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
