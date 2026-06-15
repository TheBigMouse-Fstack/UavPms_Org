namespace UavPms.Core.Interfaces.Repositories;

// Ràng buộc 'where T : class' để đảm bảo rằng T luôn là một Entity (Drone, Alert, AppUser,...)
public interface IGenericRepository<T> where T : class
{
    Task<T> GetByIdAsync(Guid id);
    Task<IReadOnlyList<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
}