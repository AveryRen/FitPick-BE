namespace FitPick_EXE201.Models.DTOs
{
    public class ResetPasswordDto
    {
        public string Email { get; set; } = null!;
        public string VerificationCode { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }
}
