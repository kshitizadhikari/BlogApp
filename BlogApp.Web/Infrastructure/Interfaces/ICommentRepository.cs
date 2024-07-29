using BlogApp.Web.Models.Entities;

namespace BlogApp.Web.Infrastructure.Interfaces
{
    public interface ICommentRepository: IBaseRepository<Comment>
    {
        Task<string> GetCommentorName(int id);
    }
}
