public class MessageViewModel
{
    public string? Username { get; set; }
    public string? Content { get; set; }
    public string? Text => Content;  // Add Text as an alias for Content
    public DateTime PublishDate { get; set; } = DateTime.UtcNow;
}
