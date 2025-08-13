namespace FitPick_EXE201.Models.Requests
{
    public class HealthprofileRequest
    {
        public List<int>? Allergies { get; set; }
        public List<int>? Chronicdiseases { get; set; }
        public List<int>? Religiondiet { get; set; }
        public List<int>? Dietarypreferences { get; set; }
        public int? Healthgoalid { get; set; }
        public int? Lifestyleid { get; set; }
        public int? Dailymeals { get; set; }
        //public int? Targetcalories { get; set; }
        public double? Height { get; set; } // cm
        public double? Weight { get; set; } // kg
        public int? Age { get; set; }        // tuổi
    }
}
