using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebNavigationTestProject.Areas.Area51.Controllers
{
    [Area("Area51")]
    [Authorize(Policy = "EmployeesOnly")]
    public class HomeController : Controller
    {
        [Authorize(Policy = "Over21")]
        public IActionResult Index()
        {
            ViewData["Message"] = "Your area description page.";

            return View();
        }
    }
}
