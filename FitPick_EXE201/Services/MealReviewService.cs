using FitPick_EXE201.Models.DTOs;
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

        public async Task<PagedResult<MealReview>> GetMealReviewsPaginatedAsync(int mealId, int page = 1, int pageSize = 10)
        {
            return await _repo.GetMealReviewsPaginatedAsync(mealId, page, pageSize);
        }

        public async Task<MealRatingStatsDto> GetMealRatingStatsAsync(int mealId)
        {
            var reviews = await _repo.GetMealReviewsAsync(mealId);
            var reviewsWithRating = reviews.Where(r => r.Rating.HasValue).ToList();

            if (!reviewsWithRating.Any())
            {
                return new MealRatingStatsDto
                {
                    AverageRating = 0,
                    TotalReviews = 0,
                    RatingDistribution = new Dictionary<int, int>()
                };
            }

            var averageRating = reviewsWithRating.Average(r => r.Rating!.Value);
            var ratingDistribution = reviewsWithRating
                .GroupBy(r => r.Rating!.Value)
                .ToDictionary(g => g.Key, g => g.Count());

            return new MealRatingStatsDto
            {
                AverageRating = Math.Round(averageRating, 1),
                TotalReviews = reviewsWithRating.Count,
                RatingDistribution = ratingDistribution
            };
        }

        public async Task<MealReview> CreateReviewAsync(MealReviewCreateDto dto, int userId)
        {
            var review = new MealReview
            {
                Mealid = dto.MealId,
                Userid = userId,
                Rating = dto.Rating,
                Comment = dto.Comment,
                Createdat = DateTime.Now
            };

            try
            {
                return await _repo.CreateReviewAsync(review);
            }
            catch (InvalidOperationException)
            {
                 return await _repo.UpdateReviewAsync(review);
            }
        }

        public async Task<MealReview> UpdateReviewAsync(int userId, int mealId, MealReviewDto dto)
        {
            var review = new MealReview
            {
                Userid = userId,
                Mealid = mealId,
                Rating = dto.Rating,
                Comment = dto.Comment,
                Updatedat = DateTime.Now
            };

            return await _repo.UpdateReviewAsync(review);
        }

        public async Task<MealReview?> GetUserReviewAsync(int userId, int mealId)
        {
            return await _repo.GetUserReviewAsync(userId, mealId);
        }

        public async Task DeleteReviewAsync(int userId, int mealId)
        {
            await _repo.DeleteReviewAsync(userId, mealId);
        }
    }
} 
