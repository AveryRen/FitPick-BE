namespace FitPick_EXE201.Repositories.Interface
{
    public interface IBaseRepo<T, TKey> where T : class
    {
        Task<T> CreateAsync(T entity);
        Task<bool> UpdateAsync(TKey id, T entity);
        Task<bool> Delete(TKey id);
        Task<T?> GetByIdAsync(TKey id);
        Task<List<T>> GetAllAsync();
    }
}
