using BlogApp.Web.Data;
using BlogApp.Web.Infrastructure.Interfaces;
using BlogApp.Web.Models.Entities;

namespace BlogApp.Web.Infrastructure.Repositories
{
    public class CommentRepository : BaseRepository<Comment>, ICommentRepository
    {
        public CommentRepository(AppDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<string> GetCommentorName(int id)
        {
            Comment? obj = await _dbContext.Comments.FindAsync(id);
            AppUser? commentor = await _dbContext.Users.FindAsync(obj.AppUserId);

            return commentor.UserName;
        }
    }
}
