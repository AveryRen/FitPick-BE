using FitPick_EXE201.Repositories.Interface;

namespace FitPick_EXE201.Services
{
    public interface IFilterService
    {
        // Get all active categories
        Task<List<object>> GetCategoriesAsync();
        
        // Get all active ingredients with pagination
        Task<List<object>> GetIngredientsAsync(int page = 0, int pageSize = 20);
        
        // Get all diet types from meals
        Task<List<object>> GetDietTypesAsync();
        
        // Get cooking time ranges
        Task<List<object>> GetCookingTimesAsync();
        
        // Get meal types (breakfast, lunch, dinner)
        Task<List<object>> GetMealTypesAsync();
        
        // Get user's specific diet plan
        Task<List<object>> GetUserDietPlansAsync(int userId);
        
        // Get suggested meals (popular meals)
        Task<List<object>> GetSuggestedMealsAsync(int limit = 10);
        
        // Search meals with filters
        Task<(List<object> meals, int totalCount)> SearchMealsWithFiltersAsync(FilterSearchRequest request);
        
        // Search meals with personal nutrition
        Task<List<object>> SearchMealsWithPersonalNutritionAsync(PersonalNutritionSearchRequest request, int userId);
    }
}
