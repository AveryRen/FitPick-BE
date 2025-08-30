using FitPick_EXE201.Models.Entities;

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
        Task<MealReview?> GetUserReviewAsync(int userId, int mealId);
        Task AddOrUpdateReviewAsync(MealReview review);
        Task DeleteReviewAsync(int userId, int mealId);

        // Save changes
        Task SaveChangesAsync();
    }
}
