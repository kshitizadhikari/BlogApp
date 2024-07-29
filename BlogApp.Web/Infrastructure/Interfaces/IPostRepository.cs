
using BlogApp.Web.Models.Entities;

namespace BlogApp.Web.Infrastructure.Interfaces
{
    public interface IPostRepository: IBaseRepository<Post>
    {
        Task<List<Comment>> GetComments(int postId);
    }
}
