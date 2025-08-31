using FitPick_EXE201.Data;
using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Repositories.Repo
{
    public class MealReviewRepo : IMealReviewRepo
    {
        private readonly FitPickContext _context;

        public MealReviewRepo(FitPickContext context)
        {
            _context = context;
        }

        // FAVORITES
        public async Task<IEnumerable<MealReview>> GetUserFavoritesAsync(int userId)
        {
            return await _context.MealReviews
                .Include(m => m.Meal) // load meal info
                .Where(r => r.Userid == userId && r.IsFavorite == true)
                .ToListAsync();
        }

        public async Task<MealReview?> GetFavoriteAsync(int userId, int mealId)
        {
            return await _context.MealReviews
                .FirstOrDefaultAsync(r => r.Userid == userId && r.Mealid == mealId && r.IsFavorite == true);
        }

        public async Task AddFavoriteAsync(MealReview favorite)
        {
            // Nếu record đã tồn tại → update IsFavorite = true
            var existing = await _context.MealReviews
                .FirstOrDefaultAsync(r => r.Userid == favorite.Userid && r.Mealid == favorite.Mealid);

            if (existing != null)
            {
                existing.IsFavorite = true;
                existing.Updatedat = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
                _context.MealReviews.Update(existing);
            }
            else
            {
                favorite.IsFavorite = true;
                favorite.Createdat = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
                _context.MealReviews.Add(favorite);
            }
        }

        public async Task RemoveFavoriteAsync(int userId, int mealId)
        {
            var existing = await _context.MealReviews
                .FirstOrDefaultAsync(r => r.Userid == userId && r.Mealid == mealId);

            if (existing != null)
            {
                existing.IsFavorite = false;
                existing.Updatedat = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
                _context.MealReviews.Update(existing);
            }
        }

        // REVIEWS
        public async Task<IEnumerable<MealReview>> GetMealReviewsAsync(int mealId)
        {
            return await _context.MealReviews
                .Include(r => r.User) // load user info
                .Where(r => r.Mealid == mealId && r.Rating != null) // chỉ lấy review có rating
                .ToListAsync();
        }

        public async Task<MealReview?> GetUserReviewAsync(int userId, int mealId)
        {
            return await _context.MealReviews
                .FirstOrDefaultAsync(r => r.Userid == userId && r.Mealid == mealId);
        }

        public async Task<MealReview> CreateReviewAsync(MealReview review)
        {
            var existing = await _context.MealReviews
                .FirstOrDefaultAsync(r => r.Userid == review.Userid && r.Mealid == review.Mealid);

            if (existing != null)
                throw new InvalidOperationException("Review already exists. Use Update instead.");

            review.Createdat = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified); ;
            _context.MealReviews.Add(review);
            await _context.SaveChangesAsync();
            return review;
        }

        public async Task<MealReview> UpdateReviewAsync(MealReview review)
        {
            var existing = await _context.MealReviews
                .FirstOrDefaultAsync(r => r.Userid == review.Userid && r.Mealid == review.Mealid);

            if (existing == null)
                throw new KeyNotFoundException("Review not found");

            existing.Rating = review.Rating;
            existing.Comment = review.Comment;
            existing.Updatedat = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified); ;

            _context.MealReviews.Update(existing);
            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task DeleteReviewAsync(int userId, int mealId)
        {
            var existing = await _context.MealReviews
                .FirstOrDefaultAsync(r => r.Userid == userId && r.Mealid == mealId);

            if (existing == null)
                throw new KeyNotFoundException("Review not found");

            _context.MealReviews.Remove(existing);
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
