namespace FitPick_EXE201.Models.DTOs
{
    public class AccountRegisterDto
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string ConfirmPassword { get; set; } = null!;
    }
}
