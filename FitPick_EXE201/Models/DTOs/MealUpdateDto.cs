using System.ComponentModel.DataAnnotations;

namespace FitPick_EXE201.Models.DTOs
{
    public class MealUpdateDto
    {
        [Required]
        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public int? Calories { get; set; }

        public int? Cookingtime { get; set; }

        public int? CategoryId { get; set; }

        public string? Diettype { get; set; }

        public decimal? Price { get; set; }

        public int? StatusId { get; set; }
    }
}
