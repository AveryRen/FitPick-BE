namespace FitPick_EXE201.Models.Requests
{
    public class CreateUserRequest
    {
        public string FullName { get; set; }
        public string Email { get; set; }   // Bắt buộc, unique
        public string Password { get; set; } // Plain text, sẽ hash ở server
        public int? GenderId { get; set; }  // Có thể null nếu chưa chọn
        public int? Age { get; set; }
        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public int RoleId { get; set; }
        public IFormFile? Avatar { get; set; }
    }
}
