using System.ComponentModel.DataAnnotations;

namespace FitPick_EXE201.Models.DTOs
{
    public class MealReviewCreateDto
    {
        [Required]
        public int MealId { get; set; }

        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        public string? Comment { get; set; } 
    }
}
