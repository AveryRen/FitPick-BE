namespace FitPick_EXE201.Models.DTOs
{
    public class UserProfileDto
    {
        public string? Fullname { get; set; }
        public string Email { get; set; }
        public int? GenderId { get; set; }
        public int? Age { get; set; }
        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }
        public string? Country { get; set; }
        public string? AvatarUrl { get; set; }
        public string? RoleName { get; set; }
    }
}
