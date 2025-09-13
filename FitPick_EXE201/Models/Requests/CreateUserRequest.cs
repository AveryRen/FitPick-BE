using System.ComponentModel.DataAnnotations;

namespace FitPick_EXE201.Models.Requests
{
    public class CreateUserRequest
    {
        [Required]
        public string FullName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
        public int? GenderId { get; set; }
        public int RoleId { get; set; }
        public int? Age { get; set; }
        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
    }
}
