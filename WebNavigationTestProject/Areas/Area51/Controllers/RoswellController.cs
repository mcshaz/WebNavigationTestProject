using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebNavigationTestProject.Areas.Area51.Controllers
{
    [Area("Area51")]
    [Authorize(Policy = "EmployeesOnly")]
    public class RoswellController : Controller
    {
        public RoswellController()
        {

        }

        public IActionResult Index()
        {
            return View();
        }
        [Authorize(Policy = "Over21")]
        public IActionResult Aliens()
        {
            return View();
        }

        [Authorize(Policy = "Over21")]
        [HttpPost]
        public IActionResult Aliens(int id)
        {
            return View();
        }

        public IActionResult MenInBlack()
        {
            return View();
        }

    }
}
