namespace FitPick_EXE201.Models.DTOs
{
    public class UserProfileDto
    {
        public int? Id { get; set; }
        public string? Fullname { get; set; }
        public string Email { get; set; }
        public int? GenderId { get; set; }
        public int? Age { get; set; }
        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }
        public string? Country { get; set; }
        public string? AvatarUrl { get; set; }
        public string? RoleName { get; set; }
        public int? RoleId { get; set; }
        public string? AccountType { get; set; } // "FREE" or "PRO"
        public bool? IsEmailVerified { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? Gender { get; set; } // String representation of gender
        public int? TargetWeight { get; set; }
        public string? DietPlan { get; set; }
        public string? CookingLevel { get; set; }
        public string? Goal { get; set; }
        public string? OtherGoal { get; set; }
        public string? ActivityLevel { get; set; }
        public bool? IsOnboardingCompleted { get; set; }
        public int? TargetCalories { get; set; }
    }
}
