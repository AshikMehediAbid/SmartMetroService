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

    public async Task<List<T>> GetAllAsync()
    {
        var result = await _dbSet.ToListAsync();

        return result;
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        var entity = await _dbSet.FindAsync(id);

        return entity;
    }

    public async Task<T> UpdateAsync(T entity)
    {
       _dbSet.Update(entity);
        return entity;
    }
}
