using FitPick_EXE201.Models.DTOs;
using FitPick_EXE201.Repositories.Interface;

namespace FitPick_EXE201.Services
{
    public class MealDetailService : IMealDetailService
    {
        private readonly IMealDetailRepository _mealDetailRepository;

        public MealDetailService(IMealDetailRepository mealDetailRepository)
        {
            _mealDetailRepository = mealDetailRepository;
        }

        public async Task<MealDetailDto?> GetMealDetailByIdAsync(int mealId)
        {
            try
            {
                // Lấy thông tin cơ bản của món ăn
                var mealDetail = await _mealDetailRepository.GetMealDetailByIdAsync(mealId);
                if (mealDetail == null) return null;

                // Lấy danh sách nguyên liệu
                var ingredients = await _mealDetailRepository.GetMealIngredientsAsync(mealId);
                mealDetail.Ingredients = ingredients;

                // Lấy hướng dẫn nấu
                var instructions = await _mealDetailRepository.GetMealInstructionsAsync(mealId);
                mealDetail.Instructions = instructions;

                return mealDetail;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
