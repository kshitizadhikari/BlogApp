using BlogApp.Web.Extensions;
using BlogApp.Web.Infrastructure.Interfaces;
using BlogApp.Web.Models;
using BlogApp.Web.Models.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BlogApp.Web.Controllers
{
    public class UserController : Controller
    {

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRepositoryWrapper _repositoryWrapper;

        public UserController(IHttpContextAccessor httpContextAccessor, IRepositoryWrapper repositoryWrapper)
        {
            _httpContextAccessor = httpContextAccessor;
            _repositoryWrapper = repositoryWrapper;
        }

        public IActionResult Home()
        {
            return View("Home");
        }

        public async Task<IActionResult> CreatePost()
        {
            return View("CreatePost");
        }

        [HttpPost]
        public async Task<IActionResult> CreatePost(CreatePostVM obj)
        {
            var curUserId = _httpContextAccessor.HttpContext.User.GetUserId();
            if (!ModelState.IsValid) return View("Home", obj);

            Post post = new Post
            {
                Title = obj.Title,
                Content = obj.Content,
                AppUserId = curUserId
            };

            _repositoryWrapper.Post.Create(post);
            await _repositoryWrapper.Save();

            return RedirectToAction("Home");
        }
    }
}
