using Microsoft.AspNetCore.Mvc;

namespace WebNavigationTestProject.Areas.Area51.Controllers
{
    [Area("Area51")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Message"] = "Your area description page.";

            return View();
        }
    }
}
