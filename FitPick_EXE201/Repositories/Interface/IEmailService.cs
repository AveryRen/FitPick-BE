namespace FitPick_EXE201.Repositories.Interface
{
    public interface IEmailService
    {
        Task SendAsync(string toEmail, string subject, string body);

    }
}
