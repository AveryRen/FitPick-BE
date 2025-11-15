using FitPick_EXE201.Repositories.Interface;

namespace FitPick_EXE201.Services
{
    public class FilterService : IFilterService
    {
        private readonly IFilterRepository _filterRepository;

        public FilterService(IFilterRepository filterRepository)
        {
            _filterRepository = filterRepository;
        }

        public async Task<List<object>> GetCategoriesAsync()
        {
            try
            {
                return await _filterRepository.GetCategoriesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting categories: {ex.Message}", ex);
            }
        }

        public async Task<List<object>> GetMealStatusesAsync()
        {
            try
            {
                return await _filterRepository.GetMealStatusesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting meal statuses: {ex.Message}", ex);
            }
        }

        public async Task<List<object>> GetIngredientsAsync(int page = 0, int pageSize = 20)
        {
            try
            {
                return await _filterRepository.GetIngredientsAsync(page, pageSize);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting ingredients: {ex.Message}", ex);
            }
        }

        public async Task<List<object>> GetDietTypesAsync()
        {
            try
            {
                return await _filterRepository.GetDietTypesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting diet types: {ex.Message}", ex);
            }
        }

        public async Task<List<object>> GetCookingTimesAsync()
        {
            try
            {
                return await _filterRepository.GetCookingTimesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting cooking times: {ex.Message}", ex);
            }
        }

        public async Task<List<object>> GetMealTypesAsync()
        {
            try
            {
                return await _filterRepository.GetMealTypesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting meal types: {ex.Message}", ex);
            }
        }

        public async Task<List<object>> GetUserDietPlansAsync(int userId)
        {
            try
            {
                return await _filterRepository.GetUserDietPlansAsync(userId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting user diet plans: {ex.Message}", ex);
            }
        }

        public async Task<List<object>> GetSuggestedMealsAsync(int limit = 10)
        {
            try
            {
                return await _filterRepository.GetSuggestedMealsAsync(limit);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting suggested meals: {ex.Message}", ex);
            }
        }

        public async Task<List<object>> GetPopularMealsAsync(int limit = 10)
        {
            try
            {
                return await _filterRepository.GetPopularMealsAsync(limit);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting popular meals: {ex.Message}", ex);
            }
        }

        public async Task<(List<object> meals, int totalCount)> SearchMealsWithFiltersAsync(FilterSearchRequest request)
        {
            try
            {
                return await _filterRepository.SearchMealsWithFiltersAsync(request);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error searching meals with filters: {ex.Message}", ex);
            }
        }

        public async Task<List<object>> SearchMealsWithPersonalNutritionAsync(PersonalNutritionSearchRequest request, int userId)
        {
            try
            {
                return await _filterRepository.SearchMealsWithPersonalNutritionAsync(request, userId);
            }
            catch (ArgumentException)
            {
                throw; // Re-throw validation exceptions
            }
            catch (Exception ex)
            {
                throw new Exception($"Error searching meals with personal nutrition: {ex.Message}", ex);
            }
        }
    }
}
