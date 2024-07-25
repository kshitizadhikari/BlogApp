namespace BlogApp.Web.Infrastructure.Interfaces
{
    public interface IRepositoryWrapper
    {
        IAppUserRepository AppUser { get; }
        IPostRepository Post { get; }
        ICommentRepository Comment { get; }
        Task Save();
    }
}
