namespace SmartMetroService.Application.Interfaces.IRepositories;

public interface IRepository<T> where T : class
{
    Task<T> GetByIdAsync(Guid id);
    Task<T> UpdateAsync(T entity);
    Task<T> DeleteAsync(Guid id);
    Task DeleteAsync(T entity);
    Task<T> AddAsync(T entity);
    Task<List<T>> GetAllAsync();
}
