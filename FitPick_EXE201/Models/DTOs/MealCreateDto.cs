namespace FitPick_EXE201.Models.DTOs
{
    public class MealCreateDto
    {
        public string Name { get; set; } = null!;
        public int CategoryId { get; set; }

         public string? Description { get; set; }
        public int? Calories { get; set; }
        public int? Cookingtime { get; set; }
        public string? Diettype { get; set; }
        public decimal? Price { get; set; }
        public int? StatusId { get; set; } 
        public List<string>? Instructions { get; set; }
    }
}
