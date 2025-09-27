using FitPick_EXE201.Models.Entities;

namespace FitPick_EXE201.Repositories.Interface
{
    public interface IReminderRepo
    {
        Task<Notification> CreateAsync(Notification reminder);
        Task<List<Notification>> GetByUserIdAsync(int userId);
        Task<Notification?> GetByIdAsync(int id, int userId);
        Task<bool> UpdateAsync(Notification reminder);
        Task<bool> DeleteAsync(Notification reminder);
    }
}
