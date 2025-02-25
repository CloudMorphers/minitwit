namespace MiniTwit.Models
{
    public class Message
    {
        public string User { get; set; } = string.Empty; // Prevent null warnings
        public string Content { get; set; } = string.Empty; // Prevent null warnings
    }
}
