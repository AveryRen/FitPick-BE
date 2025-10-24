namespace FitPick_EXE201.Models.DTOs
{
    public class UserProfileDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public int Age { get; set; }
        public int Height { get; set; }
        public int Weight { get; set; }
        public int TargetWeight { get; set; }
        public string Goal { get; set; } = string.Empty;
        public string? OtherGoal { get; set; }
        public string ActivityLevel { get; set; } = string.Empty;
        public string DietPlan { get; set; } = string.Empty;
        public string CookingLevel { get; set; } = string.Empty;
        public bool IsOnboardingCompleted { get; set; }
        public string? AvatarUrl { get; set; }
        public string AccountType { get; set; } = "FREE"; // FREE, PRO
        public string? SubscriptionType { get; set; } // Monthly, Yearly, etc.
        public string? Country { get; set; }
        public int? TargetCalories { get; set; }
    }
}
