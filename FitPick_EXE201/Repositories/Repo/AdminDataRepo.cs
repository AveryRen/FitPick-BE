using FitPick_EXE201.Data;
using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Repositories.Repo
{
    public class AdminDataRepo : IAdminDataRepo
    {
        private readonly FitPickContext _context;

        public AdminDataRepo(FitPickContext context)
        {
            _context = context;
        }

        public async Task<bool> SeedHealthGoalsAsync()
        {
            try
            {
                if (!await _context.Healthgoals.AnyAsync())
                {
                    var healthGoals = new List<Healthgoal>
                    {
                        new Healthgoal { Name = "Giảm cân", CalorieAdjustment = -500 },
                        new Healthgoal { Name = "Tăng cân", CalorieAdjustment = 500 },
                        new Healthgoal { Name = "Duy trì cân nặng", CalorieAdjustment = 0 },
                        new Healthgoal { Name = "Tăng cơ", CalorieAdjustment = 300 },
                        new Healthgoal { Name = "Ăn uống lành mạnh", CalorieAdjustment = 0 },
                        new Healthgoal { Name = "healthy", CalorieAdjustment = 0 },
                        new Healthgoal { Name = "lose", CalorieAdjustment = -500 },
                        new Healthgoal { Name = "gain", CalorieAdjustment = 300 },
                        new Healthgoal { Name = "gain weight", CalorieAdjustment = 500 },
                        new Healthgoal { Name = "target", CalorieAdjustment = 0 },
                        new Healthgoal { Name = "lose weight", CalorieAdjustment = -500 },
                        new Healthgoal { Name = "gain muscle", CalorieAdjustment = 300 }
                    };
                    _context.Healthgoals.AddRange(healthGoals);
                    await _context.SaveChangesAsync();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SeedLifestylesAsync()
        {
            try
            {
                if (!await _context.Lifestyles.AnyAsync())
                {
                    var lifestyles = new List<Lifestyle>
                    {
                        new Lifestyle { Name = "Ít vận động", Multiplier = 1.2m },
                        new Lifestyle { Name = "Vận động nhẹ", Multiplier = 1.375m },
                        new Lifestyle { Name = "Vận động vừa phải", Multiplier = 1.55m },
                        new Lifestyle { Name = "Vận động nhiều", Multiplier = 1.725m },
                        new Lifestyle { Name = "Vận động rất nhiều", Multiplier = 1.9m },
                        new Lifestyle { Name = "sedentary", Multiplier = 1.2m },
                        new Lifestyle { Name = "lightly active", Multiplier = 1.375m },
                        new Lifestyle { Name = "moderately active", Multiplier = 1.55m },
                        new Lifestyle { Name = "very active", Multiplier = 1.725m },
                        new Lifestyle { Name = "extra active", Multiplier = 1.9m }
                    };
                    _context.Lifestyles.AddRange(lifestyles);
                    await _context.SaveChangesAsync();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SeedDietPlansAsync()
        {
            try
            {
                if (!await _context.DietPlans.AnyAsync())
                {
                    var dietPlans = new List<DietPlan>
                    {
                        new DietPlan { Name = "Cân bằng", Description = "Cung cấp đầy đủ các nhóm chất dinh dưỡng thiết yếu với tỷ lệ hợp lý, giúp duy trì sức khỏe.", Status = true, CreatedAt = DateTime.Now },
                        new DietPlan { Name = "Ít tinh bột", Description = "Hạn chế tiêu thụ tinh bột (cơm, bánh mì, mì ống, đường, khoai...), tăng cường protein và chất béo.", Status = true, CreatedAt = DateTime.Now },
                        new DietPlan { Name = "Keto", Description = "Giảm thiểu tinh bột xuống mức rất thấp, tăng cường chất béo, protein vừa phải.", Status = true, CreatedAt = DateTime.Now },
                        new DietPlan { Name = "Không gluten", Description = "Tránh các thực phẩm chứa gluten (lúa mì, lúa mạch, lúa mạch đen) phù hợp cho người dị ứng hoặc nhạy cảm.", Status = true, CreatedAt = DateTime.Now },
                        new DietPlan { Name = "Không sữa", Description = "Tránh toàn bộ sản phẩm từ sữa và chế phẩm của sữa (phô mai, bơ, sữa chua, kem...).", Status = true, CreatedAt = DateTime.Now },
                        new DietPlan { Name = "Paleo", Description = "Tập trung vào thực phẩm tự nhiên, chưa qua chế biến như thịt nạc, cá, rau củ, trái cây, hạt.", Status = true, CreatedAt = DateTime.Now },
                        new DietPlan { Name = "Vegetarian", Description = "Chế độ ăn chay không thịt, cá nhưng có thể bao gồm trứng, sữa.", Status = true, CreatedAt = DateTime.Now },
                        new DietPlan { Name = "Vegan", Description = "Chế độ ăn thuần chay, loại bỏ tất cả sản phẩm từ động vật.", Status = true, CreatedAt = DateTime.Now },
                        new DietPlan { Name = "Mediterranean", Description = "Tập trung vào rau củ, trái cây, ngũ cốc nguyên hạt, dầu ô liu, cá và hạn chế thịt đỏ.", Status = true, CreatedAt = DateTime.Now },
                        new DietPlan { Name = "Intermittent Fasting", Description = "Chế độ ăn gián đoạn, tập trung vào thời gian ăn và nhịn ăn.", Status = true, CreatedAt = DateTime.Now }
                    };
                    _context.DietPlans.AddRange(dietPlans);
                    await _context.SaveChangesAsync();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SeedCookingLevelsAsync()
        {
            try
            {
                if (!await _context.CookingLevels.AnyAsync())
                {
                    var cookingLevels = new List<CookingLevel>
                    {
                        new CookingLevel { Name = "Sơ cấp", Description = "Bạn nấu được những món đơn giản, ít bước chuẩn bị và thường làm theo công thức.", Status = true, CreatedAt = DateTime.Now },
                        new CookingLevel { Name = "Trung cấp", Description = "Bạn tự tin hơn với món phức tạp, biết kết hợp nguyên liệu và linh hoạt điều chỉnh công thức.", Status = true, CreatedAt = DateTime.Now },
                        new CookingLevel { Name = "Nâng cao", Description = "Bạn thành thạo kỹ thuật nấu ăn, sáng tạo món mới và kiểm soát tốt nguyên liệu, gia vị, thời gian.", Status = true, CreatedAt = DateTime.Now }
                    };
                    _context.CookingLevels.AddRange(cookingLevels);
                    await _context.SaveChangesAsync();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<Healthgoal>> GetHealthGoalsAsync()
        {
            return await _context.Healthgoals.ToListAsync();
        }

        public async Task<IEnumerable<Lifestyle>> GetLifestylesAsync()
        {
            return await _context.Lifestyles.ToListAsync();
        }

        public async Task<IEnumerable<DietPlan>> GetDietPlansAsync()
        {
            return await _context.DietPlans.ToListAsync();
        }

        public async Task<IEnumerable<CookingLevel>> GetCookingLevelsAsync()
        {
            return await _context.CookingLevels.ToListAsync();
        }
    }
}
