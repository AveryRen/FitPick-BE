using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Repositories.Interface;

namespace FitPick_EXE201.Services
{
    public class UserIngredientService
    {
        private readonly IUserIngredientRepo _repo;
        private readonly IAdminIngredientRepo _adminRepo;

        public UserIngredientService(IUserIngredientRepo repo, IAdminIngredientRepo adminRepo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _adminRepo = adminRepo ?? throw new ArgumentNullException(nameof(adminRepo));
        }

        /// <summary>
        /// Thêm nguyên liệu mới cho user. Nếu user đã có ingredient → trả về record hiện tại
        /// </summary>
        public async Task<UserIngredient> AddOrGetAsync(int ingredientId, int userId)
        {
            if (ingredientId <= 0) throw new ArgumentException("Invalid ingredientId");
            if (userId <= 0) throw new ArgumentException("Invalid userId");

            // 1. Check ingredient tồn tại ở admin
            var ingredient = await _adminRepo.GetByIdAsync(ingredientId);
            if (ingredient == null)
                throw new InvalidOperationException("Ingredient không tồn tại");

            // 2. Check user đã có ingredient chưa
            var existing = await _repo.GetByUserIdAndIngredientIdAsync(userId, ingredientId);
            if (existing != null) return existing;

            // 3. Nếu chưa có → tạo mới
            var newEntity = new UserIngredient
            {
                Ingredientid = ingredient.Ingredientid,
                Userid = userId,
                Quantity = 0,
                Updatedat = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };

            return await _repo.AddAsync(newEntity);
        }

        public async Task<UserIngredient?> GetByIdAsync(int id)
        {
            if (id <= 0) return null;
            return await _repo.GetByIdAsync(id);
        }

        public async Task<IEnumerable<UserIngredient>> GetByUserIdAsync(int userId)
        {
            if (userId <= 0) return Enumerable.Empty<UserIngredient>();
            var items = await _repo.GetByUserIdAsync(userId);
            return items?.Where(ui => ui.Quantity > 0) ?? Enumerable.Empty<UserIngredient>();
        }

        public async Task<UserIngredient?> UpdateAsync(UserIngredient entity)
        {
            if (entity == null) return null;

            var existing = await _repo.GetByIdAsync(entity.Id);
            if (existing == null) return null;

            entity.Updatedat = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            return await _repo.UpdateAsync(entity);
        }

        public async Task<bool> ResetQuantityAsync(int id)
        {
            if (id <= 0) return false;

            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return false;

            return await _repo.ResetQuantityAsync(id);
        }
    }
}
