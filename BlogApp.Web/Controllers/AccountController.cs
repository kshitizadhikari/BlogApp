using BlogApp.Web.Enums;
using BlogApp.Web.Helpers;
using BlogApp.Web.Infrastructure.Interfaces;
using BlogApp.Web.Models;
using BlogApp.Web.Models.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System.Security.Claims;

namespace BlogApp.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IRepositoryWrapper _repository;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IEmailService _emailService;
        private readonly IDistributedCache _cache;

        public AccountController(IRepositoryWrapper repository, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IEmailService emailService, IDistributedCache cache)
        {
            _repository = repository;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _cache = cache;
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

            // Find the user by email
            AppUser? user = await _userManager.FindByEmailAsync(loginVM.Email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, loginVM.Password))
            {
                ModelState.AddModelError("Email", "Invalid email or password.");
                return View(loginVM);
            }

            // Sign in the user with the specified RememberMe option
            var result = await _signInManager.PasswordSignInAsync(user, loginVM.Password, loginVM.RememberMe, lockoutOnFailure: false);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("Email", "Invalid login attempt.");
                return View(loginVM);
            }

            // Set session data
            SessionHelper.SetUserSession(user, HttpContext);

            // Handle RememberMe logic
            if (loginVM.RememberMe)
            {
                // Set cookie options
                var cookieOptions = new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddDays(7),
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict
                };
                // Set cookies
                Response.Cookies.Append("user_id", user.Id.ToString(), cookieOptions);
                Response.Cookies.Append("username", user.UserName, cookieOptions);

                // Set values in cache (Redis)
                try
                {
                    var cacheOptions = new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                    };
                    await _cache.SetStringAsync("user_id", user.Id.ToString(), cacheOptions);
                    await _cache.SetStringAsync("username", user.UserName, cacheOptions);
                }
                catch (RedisConnectionException ex)
                {
                    // Log or handle Redis connection issue
                    Console.WriteLine($"Redis connection error: {ex.Message}");
                }
            }
            else
            {
                // Remove cookies
                Response.Cookies.Delete("user_id");
                Response.Cookies.Delete("username");

                // Remove values from cache
                await _cache.RemoveAsync("user_id");
                await _cache.RemoveAsync("username");
            }

            // Redirect to the home page
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

            string subject = "Blog Register";
            string message = $"You have been successfully registered as {user.UserName}";
            await _emailService.SendEmailAsync(user.Email, subject, message);
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
            SessionHelper.ClearUserSession(HttpContext);
        }

    }
}
