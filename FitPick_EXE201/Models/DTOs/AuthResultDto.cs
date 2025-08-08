using FitPick_EXE201.Models.Entities;

namespace FitPick_EXE201.Models.DTOs
{
    public class AuthResultDto
    {
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public int ExpiresIn { get; set; }
        public User? User { get; set; } 
    }
}

