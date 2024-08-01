using BlogApp.Web.Extensions;
using BlogApp.Web.Helpers;
using BlogApp.Web.Infrastructure.Interfaces;
using BlogApp.Web.Models;
using BlogApp.Web.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Tokens.Configuration;
using System.Xml.Linq;

namespace BlogApp.Web.Controllers
{
    public class UserController : Controller
    {

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRepositoryWrapper _repository;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IDistributedCache _cache;

        public UserController(IHttpContextAccessor httpContextAccessor, IRepositoryWrapper repositoryWrapper, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IDistributedCache cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _repository = repositoryWrapper;
            _userManager = userManager;
            _signInManager = signInManager;
            _cache = cache;
        }

        public async Task<IActionResult> Home(int pg=1)
        {
            // check cache first
            var userId = await _cache.GetStringAsync("user_id");
            var username = await _cache.GetStringAsync("username");

            if (string.IsNullOrEmpty(userId))
            {

                // check cookie
                userId = Request.Cookies["user_id"] ?? "";
                username = Request.Cookies["username"] ?? "";

                // Optionally set the cache with the values from cookies
                if (!string.IsNullOrEmpty(userId))
                {
                    await _cache.SetStringAsync("user_id", userId);
                    await _cache.SetStringAsync("username", username);
                }
            }

            // If no user ID found, redirect to login
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }
            SessionHelper.SetUserSession(userId, username, HttpContext);

            return View();

        }

        public async Task<IActionResult> CreatePost()
        {
            return View("CreatePost");
        }

        [HttpPost]
        public async Task<IActionResult> CreatePost(PostVM obj)
        {
            var curUserId = HttpContext.User.GetUserId();
            if (!ModelState.IsValid) return View(obj);

            Post post = new Post
            {
                Title = obj.Title,
                Content = obj.Content,
                AppUserId = curUserId
            };

            await _repository.Post.Create(post);
            await _repository.Save();

            return RedirectToAction("Home");
        }

        public async Task<IActionResult> ViewPost(int id)
        {
            Post? post = await _repository.Post.GetPostById(id);
            ViewPostVM obj = new ViewPostVM
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                AuthorId = post.AppUserId,
                Author = post.AppUser,
                Comments = post.Comments
            };

            return View(obj);
        }

        public async Task<IActionResult> AddComment(int postId, string comment)
        {
            var userId = HttpContext.User.GetUserId();

            Comment obj = new Comment
            {
                Content = comment,
                PostId = postId,
                AppUserId = userId
            };
            var username = HttpContext.Session.GetString("username");
            await _repository.Comment.Create(obj);
            await _repository.Save();
            return Json(new
            {
                username = username,
                comment = comment
            });
        }


        public async Task<IActionResult> LoadPostDataPartialView(int pg)
        {
            List<Post> posts = await _repository.Post.GetAll().ToListAsync();

            const int pageSize = 3;

            //if user types pg less than 1 from browser.. handling it
            if (pg < 1) pg = 1;

            //get the total number of posts
            int recsCount = posts.Count();

            //identify the start index for next or prev page
            int recsSkip = (pg - 1) * pageSize;

            Pager pager = new Pager(recsCount, pg, pageSize);
            var data = posts.Skip(recsSkip).Take(pager.PageSize).ToList();

            List<ViewPostVM> postsVM = new List<ViewPostVM>();

            foreach (var item in data)
            {

                var user = await _repository.AppUser.GetById(item.AppUserId);
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
            this.ViewBag.pager = pager;
            return PartialView("_PostTablePartial", postsVM);
        }
        public async Task<IActionResult> LoadCommentsPartialView(int postId)
        {
            try
            {
                // Retrieve the comments from the repository
                List<Comment> allComments = await _repository.Post.GetComments(postId);
                List<CommentVM> comments = new List<CommentVM>();

                foreach(var item in allComments)
                {
                    string username = await _repository.Comment.GetCommentorName(item.Id);
                    CommentVM comment = new CommentVM
                    {
                        Id = item.Id,
                        Content = item.Content,
                        PostId = item.PostId,
                        UserName = item.AppUser.UserName
                    };
                    comments.Add(comment);
                }


                // Use the correct path format with a tilde (~)
                return PartialView("_CommentsPartial", comments);
            }
            catch (Exception ex)
            {
                // Log the exception (consider using a logging framework)
                Console.Error.WriteLine($"Error loading comments: {ex.Message}");

                // Return a user-friendly error message or view
                return StatusCode(500, "Internal server error while loading comments.");
            }
        }

        public async Task DeleteUserComment(int id)
        {
            Comment? comment = await _repository.Comment.GetById(id);
            await _repository.Comment.Delete(comment);
            await _repository.Save();
        }

        [HttpPost]
        public async Task EditUserComment([FromBody] EditCommentVM editCommentVM)
        {
            Comment? comment = await _repository.Comment.GetById(editCommentVM.Id);

            comment.Content = editCommentVM.Content;
            await _repository.Comment.Update(comment);
            await _repository.Save();
        }


        public async Task<IActionResult> UpdatePost(int id)
        {
            Post? post = await _repository.Post.GetById(id);
            PostVM postVM = new PostVM
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content
            };
            return View(postVM);
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePost(PostVM obj)
        {
            if (!ModelState.IsValid) return View(obj);

            Post? post = await _repository.Post.GetById(obj.Id);
            post.Title = obj.Title;
            post.Content = obj.Content;
            await _repository.Post.Update(post);
            await _repository.Save();

            return RedirectToAction("Home");
        }

        [HttpPost]
        public async Task<IActionResult> DeletePost(int id)
        {
            Post? post = await _repository.Post.GetById(id);
            await _repository.Post.Delete(post);
            await _repository.Save();
            return RedirectToAction("Home");
        }

        public async Task<IActionResult> UserProfile()
        {
            string user_id = HttpContext.Session.GetString("user_id");
            AppUser? curUser = await _repository.AppUser.GetById(user_id);
            List<Post> posts = await _repository.AppUser.GetUserPosts(user_id);
            
            UserProfile userProfile = new UserProfile
            {
                Id = user_id,
                UserName = curUser.UserName,
                Email = curUser.Email,
                Posts = posts,
                PostCount = posts.Count,
            };
            return View(userProfile);
        }

        public async Task<IActionResult> UserSettings()
        {
            return View();
        }

        public async Task<IActionResult> ChangeUserPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangeUserPassword(ChangePasswordVM changePasswordVM)
        {
            if (!ModelState.IsValid) return View(changePasswordVM);
            AppUser? curUser = await _repository.AppUser.GetById(HttpContext.Session.GetString("user_id"));
            if (curUser == null) return RedirectToAction("Home");
            if (!await _userManager.CheckPasswordAsync(curUser, changePasswordVM.CurrentPassword)) {
                ModelState.AddModelError("CurrentPassword", "Password doesn't match current password");
                return View(changePasswordVM);
            }

            var result = await _userManager.ChangePasswordAsync(curUser, changePasswordVM.CurrentPassword, changePasswordVM.ConfirmPassword);
            if(!result.Succeeded)
            {
                ModelState.AddModelError("CurrentPassword", "Couldn't udpdate password. Try again later");
                return View(changePasswordVM);
            }
            await _userManager.UpdateSecurityStampAsync(curUser);
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
    }
}
