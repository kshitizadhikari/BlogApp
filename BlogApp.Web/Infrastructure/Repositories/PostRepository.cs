﻿using BlogApp.Web.Data;
using BlogApp.Web.Infrastructure.Interfaces;
using BlogApp.Web.Models.Entities;

namespace BlogApp.Web.Infrastructure.Repositories
{
    public class PostRepository : BaseRepository<Post>, IPostRepository
    {
        public PostRepository(AppDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<List<Comment>> GetComments(int postId)
        {
            return _dbContext.Comments.Where(c => c.PostId == postId).ToList();
        }
    }
}
