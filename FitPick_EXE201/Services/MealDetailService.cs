using FitPick_EXE201.Models.DTOs;
using FitPick_EXE201.Repositories.Interface;

namespace FitPick_EXE201.Services
{
    public class MealDetailService : IMealDetailService
    {
        private readonly IMealDetailRepository _mealDetailRepository;
        private readonly ILogger<MealDetailService> _logger;

        public MealDetailService(IMealDetailRepository mealDetailRepository, ILogger<MealDetailService> logger)
        {
            _mealDetailRepository = mealDetailRepository;
            _logger = logger;
        }

        public async Task<MealDetailDto?> GetMealDetailByIdAsync(int mealId)
        {
            try
            {
                // Lấy thông tin cơ bản của món ăn
                var mealDetail = await _mealDetailRepository.GetMealDetailByIdAsync(mealId);
                if (mealDetail == null)
                {
                    _logger.LogWarning($"Meal with ID {mealId} not found");
                    return null;
                }

                // Lấy danh sách nguyên liệu
                var ingredients = await _mealDetailRepository.GetMealIngredientsAsync(mealId);
                mealDetail.Ingredients = ingredients;
                _logger.LogInformation($"Loaded {ingredients?.Count ?? 0} ingredients for meal {mealId}");

                // Lấy hướng dẫn nấu
                var instructions = await _mealDetailRepository.GetMealInstructionsAsync(mealId);
                mealDetail.Instructions = instructions;
                _logger.LogInformation($"Loaded {instructions?.Count ?? 0} instructions for meal {mealId}");
                
                if (instructions != null && instructions.Count > 0)
                {
                    _logger.LogInformation($"Instructions for meal {mealId}: {string.Join(", ", instructions.Select(i => $"Step {i.StepNumber}: {i.Instruction}"))}");
                }

                return mealDetail;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting meal detail for meal ID {mealId}");
                throw; // Re-throw to let controller handle it
            }
        }
    }
}
