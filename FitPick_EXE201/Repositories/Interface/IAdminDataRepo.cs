using FitPick_EXE201.Models.Entities;

namespace FitPick_EXE201.Repositories.Interface
{
    public interface IAdminDataRepo
    {
        Task<bool> SeedHealthGoalsAsync();
        Task<bool> SeedLifestylesAsync();
        Task<bool> SeedDietPlansAsync();
        Task<bool> SeedCookingLevelsAsync();
        Task<IEnumerable<Healthgoal>> GetHealthGoalsAsync();
        Task<IEnumerable<Lifestyle>> GetLifestylesAsync();
        Task<IEnumerable<DietPlan>> GetDietPlansAsync();
        Task<IEnumerable<CookingLevel>> GetCookingLevelsAsync();
    }
}
