namespace BlogApp.Web.Infrastructure.Interfaces
{
    public interface IBaseRepository<T> where T : class
    {
        Task Create(T entity);
        Task Update(T entity);
        Task Delete(T entity);

        IQueryable<T> GetAll();

        Task<T?> GetById(object id);

        Task Save();

    }
}
