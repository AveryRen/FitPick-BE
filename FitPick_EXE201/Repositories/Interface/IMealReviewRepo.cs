using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Models.DTOs;

namespace FitPick_EXE201.Repositories.Interface
{
    public interface IMealReviewRepo
    {        
        // Favorites
        Task<IEnumerable<MealReview>> GetUserFavoritesAsync(int userId);
        Task<MealReview?> GetFavoriteAsync(int userId, int mealId);
        Task AddFavoriteAsync(MealReview favorite);
        Task RemoveFavoriteAsync(int userId, int mealId);

        // Reviews
        Task<IEnumerable<MealReview>> GetMealReviewsAsync(int mealId);
        Task<PagedResult<MealReview>> GetMealReviewsPaginatedAsync(int mealId, int page = 1, int pageSize = 10);
        Task<MealReview?> GetUserReviewAsync(int userId, int mealId);
        Task<MealReview> CreateReviewAsync(MealReview review);
        Task<MealReview> UpdateReviewAsync(MealReview review);
        Task DeleteReviewAsync(int userId, int mealId);

        // Save changes
        Task SaveChangesAsync();
    }
}
