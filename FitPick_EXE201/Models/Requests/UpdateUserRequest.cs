namespace FitPick_EXE201.Models.Requests
{
    public class UpdateUserRequest
    {
        public string? Fullname { get; set; }
        public string? Email { get; set; }
        public int? GenderId { get; set; }
        public int? Age { get; set; }
        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public int? RoleId { get; set; }
        public bool? Status { get; set; }
    }
}
