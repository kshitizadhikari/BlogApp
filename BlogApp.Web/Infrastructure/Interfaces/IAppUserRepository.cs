using BlogApp.Web.Models.Entities;

namespace BlogApp.Web.Infrastructure.Interfaces
{
    public interface IAppUserRepository: IBaseRepository<AppUser>
    {
        Task<List<Post>> GetUserPosts(string user_id);
    }
}
