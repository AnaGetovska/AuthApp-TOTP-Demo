namespace TotpDemo.Models
{
    public class User
    {
        public int Id { get; set; }

        public string Username { get; set; }

        public string PasswordHash { get; set; }

        public string? TOTPSecret { get; set; }

        public string? Email { get; set; }

        public bool IsTOTPEnabled { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime? LastLogin { get; set; }
    }
}
