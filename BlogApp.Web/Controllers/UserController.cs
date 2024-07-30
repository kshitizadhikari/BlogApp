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
        private readonly IRepositoryWrapper _repository;

        public UserController(IHttpContextAccessor httpContextAccessor, IRepositoryWrapper repositoryWrapper)
        {
            _httpContextAccessor = httpContextAccessor;
            _repository = repositoryWrapper;
        }

        public async Task<IActionResult> Home(int pg=1)
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

            foreach(var item in data)
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
            return View(postsVM);
        }

        public async Task<IActionResult> CreatePost()
        {
            return View("CreatePost");
        }

        [HttpPost]
        public async Task<IActionResult> CreatePost(PostVM obj)
        {
            var curUserId = _httpContextAccessor.HttpContext.User.GetUserId();
            if (!ModelState.IsValid) return View(obj);

            Post post = new Post
            {
                Title = obj.Title,
                Content = obj.Content,
                AppUserId = curUserId
            };

            _repository.Post.Create(post);
            await _repository.Save();

            return RedirectToAction("Home");
        }

        public async Task<IActionResult> ViewPost(int id)
        {
            Post? post = await _repository.Post.GetById(id);
            AppUser? user = await _repository.AppUser.GetById(post.AppUserId);
            List<Comment> comments = await _repository.Post.GetComments(id);

            foreach ( var item in comments)
            {
                AppUser? commentor = await _repository.AppUser.GetById(item.AppUserId);
                item.AppUser = commentor;
            }

            ViewPostVM obj = new ViewPostVM
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                AuthorId = user.Id,
                Author = user,
                Comments = comments
            };

            return View(obj);
        }

        public async Task<IActionResult> AddComment(int postId, string comment)
        {
            var userId = _httpContextAccessor.HttpContext.User.GetUserId();

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
    }
}
