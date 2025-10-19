using FitPick_EXE201.Data;
using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Repositories.Repo
{
    public class SettingsRepository : ISettingsRepository
    {
        private readonly FitPickContext _context;

        public SettingsRepository(FitPickContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            try
            {
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> GetPrivacyPolicyContentAsync()
        {
            // You can implement database storage here
            // For now, returning hardcoded content
            await Task.Delay(1); // Simulate async operation
            
            return @"# Chính sách bảo mật FitPick

## 1. Thông tin thu thập

Chúng tôi thu thập các thông tin sau khi bạn sử dụng ứng dụng FitPick:

### Thông tin cá nhân:
- Tên đầy đủ
- Địa chỉ email
- Tuổi, giới tính
- Chiều cao, cân nặng
- Mục tiêu sức khỏe
- Thông tin về chế độ ăn uống và lối sống

### Thông tin sử dụng:
- Lịch sử bữa ăn
- Đánh giá và nhận xét về món ăn
- Dữ liệu dinh dưỡng tiêu thụ
- Thông tin hoạt động thể chất

## 2. Mục đích sử dụng

Chúng tôi sử dụng thông tin của bạn để:
- Cung cấp dịch vụ cá nhân hóa
- Đề xuất món ăn phù hợp
- Theo dõi tiến trình sức khỏe
- Cải thiện chất lượng dịch vụ

## 3. Bảo mật thông tin

Chúng tôi cam kết:
- Mã hóa dữ liệu nhạy cảm
- Không chia sẻ thông tin với bên thứ ba
- Tuân thủ các tiêu chuẩn bảo mật quốc tế
- Có biện pháp bảo vệ chống truy cập trái phép

## 4. Quyền của người dùng

Bạn có quyền:
- Truy cập và chỉnh sửa thông tin cá nhân
- Xóa tài khoản và dữ liệu
- Rút lại sự đồng ý bất kỳ lúc nào
- Yêu cầu xuất dữ liệu cá nhân

## 5. Liên hệ

Nếu có thắc mắc về chính sách bảo mật, vui lòng liên hệ:
- Email: privacy@fitpick.com
- Điện thoại: +84 123 456 789

Chính sách này có hiệu lực từ ngày 1/1/2024 và có thể được cập nhật định kỳ.";
        }

        public async Task<string> GetTermsOfServiceContentAsync()
        {
            // You can implement database storage here
            // For now, returning hardcoded content
            await Task.Delay(1); // Simulate async operation
            
            return @"# Điều khoản dịch vụ FitPick

## 1. Chấp nhận điều khoản

Bằng việc sử dụng ứng dụng FitPick, bạn đồng ý tuân thủ các điều khoản và điều kiện sau đây.

## 2. Mô tả dịch vụ

FitPick là ứng dụng hỗ trợ:
- Quản lý chế độ ăn uống và dinh dưỡng
- Đề xuất món ăn phù hợp với sức khỏe
- Theo dõi tiến trình sức khỏe cá nhân
- Cộng đồng chia sẻ kinh nghiệm ăn uống

## 3. Quyền và nghĩa vụ người dùng

### Quyền của bạn:
- Sử dụng dịch vụ theo đúng mục đích
- Nhận hỗ trợ kỹ thuật từ đội ngũ FitPick
- Đề xuất cải tiến dịch vụ

### Nghĩa vụ của bạn:
- Cung cấp thông tin chính xác
- Không sử dụng dịch vụ cho mục đích bất hợp pháp
- Tuân thủ các quy định của pháp luật
- Không chia sẻ tài khoản với người khác

## 4. Hạn chế trách nhiệm

FitPick không chịu trách nhiệm về:
- Kết quả sức khỏe từ việc áp dụng gợi ý
- Thiệt hại do sử dụng sai thông tin
- Gián đoạn dịch vụ do lỗi kỹ thuật
- Mất dữ liệu do nguyên nhân khách quan

## 5. Quyền sở hữu trí tuệ

Tất cả nội dung trong ứng dụng thuộc quyền sở hữu của FitPick:
- Thuật toán đề xuất món ăn
- Cơ sở dữ liệu dinh dưỡng
- Giao diện và thiết kế ứng dụng
- Thương hiệu và logo

## 6. Chấm dứt dịch vụ

Chúng tôi có quyền:
- Tạm dừng tài khoản vi phạm điều khoản
- Chấm dứt dịch vụ với thông báo trước 30 ngày
- Xóa tài khoản không hoạt động trong 12 tháng

## 7. Giải quyết tranh chấp

Mọi tranh chấp sẽ được giải quyết:
- Ưu tiên thương lượng hòa giải
- Trọng tài tại Trung tâm Trọng tài Quốc tế Việt Nam
- Áp dụng pháp luật Việt Nam

## 8. Liên hệ

Để được hỗ trợ về điều khoản dịch vụ:
- Email: support@fitpick.com
- Điện thoại: +84 123 456 789
- Địa chỉ: 123 Đường ABC, Quận XYZ, TP.HCM

Điều khoản này có hiệu lực từ ngày 1/1/2024.";
        }
    }
}
