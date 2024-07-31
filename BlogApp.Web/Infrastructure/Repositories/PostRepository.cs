using BlogApp.Web.Data;
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

        public async Task<Post> GetPostById(int id)
        {
            Post? post = await _dbContext.Posts.FindAsync(id);
            AppUser? author = await _dbContext.Users.FindAsync(post.AppUserId);
            List<Comment> comments = await GetComments(id);

            foreach (var comment in comments)
            {
                AppUser? commentor = await _dbContext.Users.FindAsync(comment.AppUserId);
                comment.AppUser = commentor;
            }

            post.AppUser = author;
            post.Comments = comments;
            return post;
        }
    }
}
