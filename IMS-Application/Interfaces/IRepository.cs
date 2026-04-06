using IMS_Application.Common.Models;

namespace IMS_Application.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<PagedResult<T>> GetPagedAsync(int pageNumber, int pageSize);
        Task AddAsync(T entity);
        void Update(T entity);
        void Remove(T entity);
    }
}
