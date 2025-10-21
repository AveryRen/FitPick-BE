namespace FitPick_EXE201.Models.DTOs
{
    public class MealReviewDto
    {
        public int ReviewId { get; set; }
        public int? Rating { get; set; }
        public string? Comment { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? UserAvatar { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
