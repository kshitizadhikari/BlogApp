using BlogApp.Web.Data;
using BlogApp.Web.Infrastructure.Interfaces;
using BlogApp.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlogApp.Web.Infrastructure.Repositories
{
    public class AppUserRepository : BaseRepository<AppUser>, IAppUserRepository
    {
        public AppUserRepository(AppDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<List<Post>> GetUserPosts(string user_id)
        {
            List<Post>? posts = await _dbContext.Posts.Where(p => p.AppUserId == user_id).ToListAsync();
            return posts;
        }
    }
}
