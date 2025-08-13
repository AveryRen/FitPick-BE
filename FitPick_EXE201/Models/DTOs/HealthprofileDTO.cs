namespace FitPick_EXE201.Models.DTOs
{
    public class HealthprofileDTO
    {
        public int Profileid { get; set; }
        public int? Userid { get; set; }
        public List<int>? Allergies { get; set; }
        public List<IngredientDTO>? AllergyDetails { get; set; }
        public List<int>? Chronicdiseases { get; set; }
        public List<IngredientDTO>? ChronicdiseasesDetails { get; set; }
        public List<int>? Religiondiet { get; set; }
        public List<IngredientDTO>? ReligiondietDetails { get; set; }
        public List<int>? Dietarypreferences { get; set; }
        public List<IngredientDTO>? DietarypreferencesDetails { get; set; }
        public int? Healthgoalid { get; set; }
        public string? HealthgoalName { get; set; }
        public int? HealthgoalCalorieAdjustment { get; set; }
        public int? Lifestyleid { get; set; }
        public string? LifestyleName { get; set; }
        public decimal? LifestyleMultiplier { get; set; } 
        public int? Dailymeals { get; set; }
        public int? Targetcalories { get; set; }
        public bool? Status { get; set; }
        public DateTime? Updatedat { get; set; }
    }

    public class IngredientDTO
    {
        public int Ingredientid { get; set; }
        public string Name { get; set; } = null!;
        public string? Type { get; set; }
        public string? Unit { get; set; }
        public bool? Status { get; set; }
    }
}
