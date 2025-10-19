namespace FitPick_EXE201.Models.DTOs
{
    public class AdminUserDetailDto
    {
        public string? Fullname { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? Gender { get; set; }
        public int? Age { get; set; }
        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public string? Role { get; set; }
        public bool? Status { get; set; }
        public int? GenderId { get; set; }  
        public int? RoleId { get; set; }
        public string? AvatarUrl { get; set; }
    }
}
