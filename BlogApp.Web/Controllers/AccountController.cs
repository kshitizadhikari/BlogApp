using BlogApp.Web.Enums;
using BlogApp.Web.Infrastructure.Interfaces;
using BlogApp.Web.Models;
using BlogApp.Web.Models.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
                return View(loginVM);
            }

            AppUser? user = await _userManager.FindByEmailAsync(loginVM.Email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, loginVM.Password))
            {
                ModelState.AddModelError("Email", "Invalid email or password.");
                return View(loginVM);
            }

            var result = await _signInManager.PasswordSignInAsync(user, loginVM.Password, loginVM.RememberMe, lockoutOnFailure: false);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("Email", "Invalid login attempt.");
                return View(loginVM);
            }

            SetUserSession(user);
            await SignInUserAsync(user, loginVM.RememberMe);
            return RedirectToAction("Index", "Home");
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
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "User with this email already exists.");
                return View(registerVM);
            }

            var user = new AppUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = registerVM.UserName,
                Email = registerVM.Email
            };

            var result = await _userManager.CreateAsync(user, registerVM.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(registerVM);
            }

            await _userManager.AddToRoleAsync(user, Roles.User.ToString());

            return RedirectToAction("Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        private void SetUserSession(AppUser user)
        {
            // Set session data
            HttpContext.Session.SetString("user_id", user.Id);
            HttpContext.Session.SetString("username", user.UserName);
        }

        private void ClearUserSession()
        {
            HttpContext.Session.Clear();
        }

        private async Task SignInUserAsync(AppUser user, bool isPersistent)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = isPersistent,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30) // Cookie expiration set to 30 days
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
        }

        private async Task SignOutUserAsync()
        {
            await _signInManager.SignOutAsync();
            ClearUserSession();
        }

    }
}
