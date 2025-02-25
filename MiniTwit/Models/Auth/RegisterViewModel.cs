namespace MiniTwit.Models.Auth
{
    public class RegisterViewModel
    {
        public string? Username { get; set; }  // Nullable
        public string? Email { get; set; }     // Nullable
        public string? Password { get; set; }  // Nullable
        public string? Password2 { get; set; } // Nullable
    }
}
