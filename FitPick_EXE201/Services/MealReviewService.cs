using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Repositories.Interface;

namespace FitPick_EXE201.Services
{
    public class MealReviewService
    {
        private readonly IMealReviewRepo _repo;

        public MealReviewService(IMealReviewRepo repo)
        {
            _repo = repo;
        }

        // FAVORITES
        public async Task<IEnumerable<MealReview>> GetUserFavoritesAsync(int userId)
        {
            return await _repo.GetUserFavoritesAsync(userId);
        }

        public async Task<bool> AddFavoriteAsync(int userId, int mealId)
        {
            var favorite = new MealReview
            {
                Userid = userId,
                Mealid = mealId
            };

            await _repo.AddFavoriteAsync(favorite);
            await _repo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveFavoriteAsync(int userId, int mealId)
        {
            await _repo.RemoveFavoriteAsync(userId, mealId);
            await _repo.SaveChangesAsync();
            return true;
        }
        
        // REVIEWS
        public async Task<IEnumerable<MealReview>> GetMealReviewsAsync(int mealId)
        {
            return await _repo.GetMealReviewsAsync(mealId);
        }

        public async Task<MealReview?> GetUserReviewAsync(int userId, int mealId)
        {
            return await _repo.GetUserReviewAsync(userId, mealId);
        }

        public async Task<bool> AddOrUpdateReviewAsync(int userId, int mealId, int rating, string? comment)
        {
            var review = new MealReview
            {
                Userid = userId,
                Mealid = mealId,
                Rating = rating,
                Comment = comment
            };

            await _repo.AddOrUpdateReviewAsync(review);
            await _repo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteReviewAsync(int userId, int mealId)
        {
            await _repo.DeleteReviewAsync(userId, mealId);
            await _repo.SaveChangesAsync();
            return true;
        }
    }
} 