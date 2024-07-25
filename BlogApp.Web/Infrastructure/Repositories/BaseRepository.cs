﻿using BlogApp.Web.Data;
using BlogApp.Web.Infrastructure.Interfaces;

namespace BlogApp.Web.Infrastructure.Repositories
{
    public abstract class BaseRepository<T>: IBaseRepository<T> where T : class
    {

        protected readonly AppDbContext _dbContext;

        protected BaseRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public bool Create(T entity)
        {
            _dbContext.Set<T>().Add(entity);
            return true;
        }

        public bool Update(T entity)
        {
            _dbContext.Set<T>().Update(entity);
            return true;
        }

        public bool Delete(T entity)
        {
            _dbContext.Set<T>().Remove(entity);
            return true;
        }

        public IQueryable<T> GetAll()
        {
            return _dbContext.Set<T>();
        }

        public async Task<T?> GetById(object id)
        {
            return await _dbContext.Set<T>().FindAsync(id);
        }

        public async Task Save()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}