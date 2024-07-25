using BlogApp.Web.Extensions;
using BlogApp.Web.Infrastructure.Interfaces;
using BlogApp.Web.Models;
using BlogApp.Web.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        public async Task<IActionResult> Home()
        {
            List<Post> posts = await _repositoryWrapper.Post.GetAll().ToListAsync();

            List<ViewPostVM> postsVM = new List<ViewPostVM>();

            foreach(var item in posts)
            {

                var user = await _repositoryWrapper.AppUser.GetById(item.AppUserId);
                ViewPostVM obj = new ViewPostVM
                {
                    Id = item.Id,
                    Title = item.Title,
                    Content = item.Content,
                    AuthorId = user.Id,
                    Author = user
                };
                postsVM.Add(obj);
            }

            return View("Home", postsVM);
        }

        public async Task<IActionResult> CreatePost()
        {
            return View("CreatePost");
        }

        [HttpPost]
        public async Task<IActionResult> CreatePost(CreatePostVM obj)
        {
            var curUserId = _httpContextAccessor.HttpContext.User.GetUserId();
            if (!ModelState.IsValid) return View(obj);

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

        public async Task<IActionResult> ViewPost(int id)
        {
            Post? post = await _repositoryWrapper.Post.GetById(id);
            AppUser? user = await _repositoryWrapper.AppUser.GetById(post.AppUserId);

            ViewPostVM obj = new ViewPostVM
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                AuthorId = user.Id,
                Author = user
            };

            return View(obj);
        }


    }
}
