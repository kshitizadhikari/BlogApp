using Microsoft.AspNetCore.Mvc;

namespace BlogApp.Web.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }
    }
}
