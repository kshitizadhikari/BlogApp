using BlogApp.Web.Enums;
using BlogApp.Web.Infrastructure.Interfaces;
using BlogApp.Web.Models;
using BlogApp.Web.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BlogApp.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IRepositoryWrapper _repository;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public AccountController(IRepositoryWrapper repository, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _repository = repository;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM loginVM)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("Email", "Invalid Email or Password");
                return View(loginVM);
            }

            AppUser user = await _userManager.FindByEmailAsync(loginVM.Email);

            if (user == null)
            {
                ModelState.AddModelError("Email", "Invalid Email or Password");
                return View(loginVM);
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginVM.Password, false);

            if(result == null)
            {
                ModelState.AddModelError("Email", "Invalid Email or Password");
                return View(loginVM);
            }

            await _signInManager.SignInAsync(user, true);
            return RedirectToAction("Home", "User");
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM registerVM)
        {
            if (!ModelState.IsValid) return View(registerVM);

            var existingUser = await _userManager.FindByEmailAsync(registerVM.Email);
            if(existingUser != null)
            {
                ModelState.AddModelError("Email", "User with this email already exists");
                return View(registerVM);
            }
            AppUser user = new AppUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = registerVM.UserName,
                Email = registerVM.Email
            };
            var result = await _userManager.CreateAsync(user, registerVM.Password);
            if (!result.Succeeded) return View(registerVM);

            await _userManager.AddToRoleAsync(user, Roles.User.ToString());

            return RedirectToAction("Login");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }
    }
}
