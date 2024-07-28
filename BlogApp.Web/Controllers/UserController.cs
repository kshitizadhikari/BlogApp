﻿using BlogApp.Web.Extensions;
using BlogApp.Web.Infrastructure.Interfaces;
using BlogApp.Web.Models;
using BlogApp.Web.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Nodes;

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

        public async Task<IActionResult> Home()
        {
            List<Post> posts = await _repository.Post.GetAll().ToListAsync();
            List<ViewPostVM> postsVM = new List<ViewPostVM>();

            foreach(var item in posts)
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

            return View("Home", postsVM);
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
            List<Comment> comments = _repository.Post.GetComments(id);

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
            _repository.Comment.Create(obj);
            await _repository.Save();
            return Json(new
            {
                username = username,
                comment = comment
            });
        }

        public async Task<JsonResult> GetComments(int postId)
        {
            // Retrieve the comments from the repository
            List<Comment> allComments = _repository.Post.GetComments(postId);

            // Return the comments as a JSON result
            return Json(new
            {
                comments = allComments
            });
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
            _repository.Post.Update(post);
            await _repository.Save();

            return RedirectToAction("Home");
        }

        [HttpPost]
        public async Task<IActionResult> DeletePost(int id)
        {
            Post? post = await _repository.Post.GetById(id);
            _repository.Post.Delete(post);
            await _repository.Save();
            return RedirectToAction("Home");
        }
    }
}
