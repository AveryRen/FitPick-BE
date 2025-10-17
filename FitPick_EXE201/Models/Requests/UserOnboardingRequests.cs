namespace FitPick_EXE201.Models.Requests
{
    public class SaveUserProfileRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public int Age { get; set; }
        public int Height { get; set; }
        public int Weight { get; set; }
        public int TargetWeight { get; set; }
    }

    public class SaveUserGoalsRequest
    {
        public string Goal { get; set; } = string.Empty;
        public string? OtherGoal { get; set; }
    }

    public class SaveUserLifestyleRequest
    {
        public string ActivityLevel { get; set; } = string.Empty;
    }

    public class SaveUserDietPlanRequest
    {
        public string DietPlan { get; set; } = string.Empty;
    }

    public class SaveUserCookingLevelRequest
    {
        public string CookingLevel { get; set; } = string.Empty;
    }
}
