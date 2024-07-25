namespace BlogApp.Web.Infrastructure.Interfaces
{
    public interface IBaseRepository<T> where T : class
    {
        bool Create(T entity);
        bool Update(T entity);
        bool Delete(T entity);

        IQueryable<T> GetAll();

        Task<T?> GetById(object id);

        Task Save();

    }
}
