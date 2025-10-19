using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Repositories.Interface;

namespace FitPick_EXE201.Services
{
    public interface IAdminDataService
    {
        Task<bool> SeedDataAsync();
        Task<object> CheckDataAsync();
    }

    public class AdminDataService : IAdminDataService
    {
        private readonly IAdminDataRepo _adminDataRepo;

        public AdminDataService(IAdminDataRepo adminDataRepo)
        {
            _adminDataRepo = adminDataRepo;
        }

        public async Task<bool> SeedDataAsync()
        {
            try
            {
                var healthGoalsResult = await _adminDataRepo.SeedHealthGoalsAsync();
                var lifestylesResult = await _adminDataRepo.SeedLifestylesAsync();
                var dietPlansResult = await _adminDataRepo.SeedDietPlansAsync();
                var cookingLevelsResult = await _adminDataRepo.SeedCookingLevelsAsync();

                return healthGoalsResult && lifestylesResult && dietPlansResult && cookingLevelsResult;
            }
            catch
            {
                return false;
            }
        }

        public async Task<object> CheckDataAsync()
        {
            try
            {
                var healthGoals = await _adminDataRepo.GetHealthGoalsAsync();
                var lifestyles = await _adminDataRepo.GetLifestylesAsync();
                var dietPlans = await _adminDataRepo.GetDietPlansAsync();
                var cookingLevels = await _adminDataRepo.GetCookingLevelsAsync();

                return new
                {
                    healthGoals = healthGoals.Select(hg => new { hg.Id, hg.Name }),
                    lifestyles = lifestyles.Select(l => new { l.Id, l.Name }),
                    dietPlans = dietPlans.Select(dp => new { dp.Id, dp.Name }),
                    cookingLevels = cookingLevels.Select(cl => new { cl.Id, cl.Name })
                };
            }
            catch
            {
                throw;
            }
        }
    }
}
