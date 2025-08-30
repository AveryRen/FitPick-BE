namespace FitPick_EXE201.Models.DTOs
{
    public class FavoriteMealDto
    {
        public int MealId { get; set; }
        public string MealName { get; set; } = string.Empty;
        public bool IsFavorite { get; set; }
        public int? Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
