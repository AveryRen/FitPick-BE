using AutoMapper;
using FitPick_EXE201.Data;
using FitPick_EXE201.Models.DTOs;
using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Services
{
    public class SpendinglogService
    {
        private readonly ISpendinglogRepo _repo;
        private readonly IMapper _mapper;
        private readonly FitPickContext _context;

        public SpendinglogService(ISpendinglogRepo repo, IMapper mapper, FitPickContext context)
        {
            _context = context;
            _repo = repo;
            _mapper = mapper;
        }

        /// <summary>
        /// Thêm chi tiêu thủ công
        /// </summary>
        public async Task<SpendinglogDTO> AddSpendingAsync(int userId, decimal amount, string? note = null)
        {
            var log = new Spendinglog
            {
                Userid = userId,
                Amount = amount,
                Note = note,
                Date = DateOnly.FromDateTime(DateTime.UtcNow),
            };

            var created = await _repo.CreateAsync(log);
            return _mapper.Map<SpendinglogDTO>(created);
        }

        /// <summary>
        /// Thêm chi tiêu theo 1 món ăn
        /// </summary>
        public async Task<SpendinglogDTO> AddSpendingForMealAsync(int userId, int mealId)
        {
            // lấy meal từ DB
            var meal = await _context.Meals.FindAsync(mealId);
            if (meal == null)
            {
                throw new KeyNotFoundException($"Meal with id {mealId} not found.");
            }

            var log = new Spendinglog
            {
                Userid = userId,
                Amount = meal.Price,
                Note = meal.Name,
                Date = DateOnly.FromDateTime(DateTime.UtcNow),
            };

            var created = await _repo.CreateAsync(log);
            return _mapper.Map<SpendinglogDTO>(created);
        }

        /// <summary>
        /// Thêm chi tiêu cho nhiều món ăn theo MealId
        /// </summary>
        public async Task<List<SpendinglogDTO>> AddSpendingForMealsAsync(int userId, List<int> mealIds)
        {
            var meals = await _context.Meals
                .Where(m => mealIds.Contains(m.Mealid))
                .ToListAsync();

            // Nếu có mealId nào không tồn tại thì báo lỗi
            if (meals.Count != mealIds.Count)
            {
                var missingIds = mealIds.Except(meals.Select(m => m.Mealid)).ToList();
                throw new KeyNotFoundException($"Meals not found with ids: {string.Join(", ", missingIds)}");
            }

            var results = new List<Spendinglog>();

            foreach (var meal in meals)
            {
                var log = new Spendinglog
                {
                    Userid = userId,
                    Amount = meal.Price ?? 0, // phòng null
                    Note = meal.Name,
                    Date = DateOnly.FromDateTime(DateTime.UtcNow),
                };

                var created = await _repo.CreateAsync(log);
                results.Add(created);
            }

            return _mapper.Map<List<SpendinglogDTO>>(results);
        }


        /// <summary>
        /// Lấy tổng chi tiêu theo ngày
        /// </summary>
        public async Task<decimal> GetDailyTotalAsync(int userId, DateOnly date)
        {
            var logs = await _repo.GetAllAsync();
            return logs
                .Where(s => s.Userid == userId && s.Date == date)
                .Sum(s => s.Amount ?? 0);
        }

        /// <summary>
        /// Lấy tổng chi tiêu theo tháng
        /// </summary>
        public async Task<decimal> GetMonthlyTotalAsync(int userId, int year, int month)
        {
            var logs = await _repo.GetAllAsync();
            return logs
                .Where(s => s.Userid == userId
                            && s.Date.Year == year
                            && s.Date.Month == month)
                .Sum(s => s.Amount ?? 0);
        }

        /// <summary>
        /// Lấy tổng chi tiêu theo năm
        /// </summary>
        public async Task<decimal> GetYearlyTotalAsync(int userId, int year)
        {
            var logs = await _repo.GetAllAsync();
            return logs
                .Where(s => s.Userid == userId && s.Date.Year == year)
                .Sum(s => s.Amount ?? 0);
        }

        /// <summary>
        /// Lấy log chi tiêu trong khoảng ngày
        /// </summary>
        public async Task<List<SpendinglogDTO>> GetSpendingInRangeAsync(int userId, DateOnly start, DateOnly end)
        {
            var logs = await _repo.GetAllAsync();
            var filtered = logs
                .Where(s => s.Userid == userId
                            && s.Date >= start
                            && s.Date <= end)
                .ToList();

            return _mapper.Map<List<SpendinglogDTO>>(filtered);
        }

        /// <summary>
        /// Cập nhật chi tiêu
        /// </summary>
        public async Task<SpendinglogDTO> UpdateSpendingAsync(int logId, decimal? amount = null, string? note = null)
        {
            var log = await _repo.GetByIdAsync(logId);
            if (log == null)
            {
                throw new KeyNotFoundException($"Spending log with id {logId} not found.");
            }

            if (amount.HasValue)
                log.Amount = amount.Value;
            if (!string.IsNullOrWhiteSpace(note))
                log.Note = note;

            // cập nhật ngày giờ sửa
            log.Date = DateOnly.FromDateTime(DateTime.UtcNow);

            var success = await _repo.UpdateAsync(logId, log);
            if (!success)
            {
                throw new Exception("Failed to update spending log.");
            }

            // ✅ lấy lại log sau khi update để map DTO
            var updatedLog = await _repo.GetByIdAsync(logId);
            return _mapper.Map<SpendinglogDTO>(updatedLog);
        }

        /// <summary>
        /// Xóa chi tiêu
        /// </summary>
        public async Task<bool> DeleteSpendingAsync(int logId)
        {
            var log = await _repo.GetByIdAsync(logId);
            if (log == null)
            {
                throw new KeyNotFoundException($"Spending log with id {logId} not found.");
            }

            return await _repo.Delete(logId);
        }




    }
}
