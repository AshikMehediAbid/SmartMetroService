using Microsoft.EntityFrameworkCore;
using SmartMetroService.Application.Interfaces.IRepositories;
using SmartMetroService.Storage.Sql;

namespace SmartMetroService.Storage.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    public readonly MyApplicationDbContext _db;
    public readonly DbSet<T> _dbSet;

    public Repository(MyApplicationDbContext db)
    {
        _db = db;
        _dbSet = _db.Set<T>();
    }

    public async Task<T> AddAsync(T entity)
    {
        var result = await _dbSet.AddAsync(entity);

        return result.Entity;
    }

    public Task<T> DeleteAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(T entity)
    {
        throw new NotImplementedException();
    }

    public Task<List<T>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<T> GetByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<T> UpdateAsync(T entity)
    {
        throw new NotImplementedException();
    }
}
