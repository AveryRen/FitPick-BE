using System.ComponentModel.DataAnnotations;

namespace FitPick_EXE201.Models.Requests
{
    public class AddMealToMenuRequest
    {
        [Required]
        public int MealId { get; set; }
        
        [Required]
        public DateTime Date { get; set; }
        
        public string? MealTime { get; set; } // Optional, default to breakfast
    }
}
