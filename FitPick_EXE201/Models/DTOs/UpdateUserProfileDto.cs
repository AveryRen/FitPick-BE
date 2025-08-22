namespace FitPick_EXE201.Models.DTOs
{
    public class UpdateUserProfileDto
    {
        public string? Fullname { get; set; }
        public int? GenderId { get; set; }
        public int? Age { get; set; }
        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public string? AvatarUrl { get; set; }
    }
}
