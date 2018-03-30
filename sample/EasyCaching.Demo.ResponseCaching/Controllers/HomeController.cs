namespace EasyCaching.Demo.ResponseCaching.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System;
    
    public class HomeController : Controller
    {
        [ResponseCache(Duration = 20)]
        public IActionResult Index()
        {
            return View(new Models.TestModel
            {
                LastUpdated = DateTimeOffset.UtcNow.ToString()
            });
        }

        public IActionResult About()
        {
            return View(new Models.TestModel
            {
                LastUpdated = DateTimeOffset.UtcNow.ToString()
            });
        }
    }
}
