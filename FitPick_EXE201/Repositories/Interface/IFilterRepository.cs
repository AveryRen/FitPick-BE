using FitPick_EXE201.Models.Entities;

namespace FitPick_EXE201.Repositories.Interface
{
    public interface IFilterRepository
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

    // Request model for filter search
    public class FilterSearchRequest
    {
        public int? UserId { get; set; }
        public string? DietType { get; set; }
        public int? MaxCookingTime { get; set; }
        public int? MinCalories { get; set; }
        public int? MaxCalories { get; set; }
        public List<string>? Ingredients { get; set; }
        public List<string>? Categories { get; set; }
        public List<string>? MealTypes { get; set; }
        public bool? IsPremium { get; set; }
        public bool UsePersonalNutrition { get; set; } = false;
        public int Page { get; set; } = 0;
        public int PageSize { get; set; } = 20;
    }

    // Request model for personal nutrition search
    public class PersonalNutritionSearchRequest
    {
        public string? DietType { get; set; }
        public int? MaxCookingTime { get; set; }
        public List<string>? Ingredients { get; set; }
        public List<string>? MealTypes { get; set; }
    }
}
