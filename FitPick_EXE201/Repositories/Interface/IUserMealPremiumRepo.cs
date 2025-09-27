using FitPick_EXE201.Models.Entities;

namespace FitPick_EXE201.Repositories.Interface
{
    public interface IUserMealPremiumRepo
    {
        Task<List<Meal>> GetPremiumMealsAsync();
        IQueryable<Meal> GetPremiumMealsQuery();
    }
}
