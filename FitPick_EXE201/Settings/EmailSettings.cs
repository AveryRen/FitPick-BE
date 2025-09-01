namespace FitPick_EXE201.Settings
{
    public class EmailSettings
    {
        public string From { get; set; } = null!;
        public string Host { get; set; } = null!;
        public int Port { get; set; }
        public bool UseSSL { get; set; }
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
