using FitPick_EXE201.Models.Entities;

namespace FitPick_EXE201.Repositories.Interface
{
    public interface IHealthprofileRepo : IBaseRepo<Healthprofile, int>
    {
        Task<Healthprofile?> GetByUserIdAsync(int userid);
    }
}
