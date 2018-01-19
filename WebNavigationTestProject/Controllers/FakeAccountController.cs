using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;

namespace WebNavigationTestProject.Controllers
{
    [Authorize]
    public class FakeAccountController : Controller
    {
        public FakeAccountController()
        {

        }

        // GET: /Account/index
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index()
        {
            ViewData["Title"] = "Login";
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string userName = null)
        {
            //fake login with roles to demonstrate role based menu filtering
            AuthenticationProperties authProperties = new AuthenticationProperties();
            ClaimsPrincipal user;
            switch(userName)
            {
                case "Administrator":
                    user = GetAdminClaimsPrincipal();
                    break;
                case "EmployeeOver21":
                    user = GetEmployeeClaimsPrincipal(33);
                    break;
                case "EmployeeUnder21":
                    user = GetEmployeeClaimsPrincipal(19);
                    break;
                case "Member":
                default:
                    user = GetMemberClaimsPrincipal();
                    break;
            }
            await HttpContext.SignInAsync("application", user, authProperties);

            return RedirectToAction(nameof(HomeController.Index), "Home");


            //return View("Index");
        }

        private ClaimsPrincipal GetAdminClaimsPrincipal()
        {
            var identity = new ClaimsIdentity("application");
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, "1"));
            identity.AddClaim(new Claim(ClaimTypes.Name, "Administrator"));
            identity.AddClaim(new Claim(ClaimTypes.Role, "Admins"));


            return new ClaimsPrincipal(identity);
        }

        private ClaimsPrincipal GetMemberClaimsPrincipal()
        {
            var identity = new ClaimsIdentity("application");
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, "1"));
            identity.AddClaim(new Claim(ClaimTypes.Name, "Member"));
            identity.AddClaim(new Claim(ClaimTypes.Role, "Members"));


            return new ClaimsPrincipal(identity);
        }

        private ClaimsPrincipal GetEmployeeClaimsPrincipal(int age)
        {
            var identity = new ClaimsIdentity("application");
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, "1"));
            identity.AddClaim(new Claim(ClaimTypes.Name, age + "yoEmployee"));
            identity.AddClaim(new Claim(ClaimTypes.Role, "Members"));
            identity.AddClaim(new Claim("EmployeeId", age.ToString()));
            identity.AddClaim(new Claim(ClaimTypes.DateOfBirth, (DateTime.Today.Year - age) + "-01-01"));
            return new ClaimsPrincipal(identity);
        }

        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOff()
        {
            //await _signInManager.SignOutAsync();
            await HttpContext.SignOutAsync("application");

            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }

    }
}
