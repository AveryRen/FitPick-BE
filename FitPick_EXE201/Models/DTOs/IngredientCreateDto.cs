using System.ComponentModel.DataAnnotations;

namespace FitPick_EXE201.Models.DTOs
{
    public class IngredientCreateDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;

        [StringLength(50)]
        public string? Type { get; set; }

        [StringLength(20)]
        public string? Unit { get; set; }
    }
}
