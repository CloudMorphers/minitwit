namespace MiniTwit.Data;

public class Message
{
    public int Id { get; set; }

    public string Text { get; set; } = string.Empty;

    public DateTime PublishDate { get; set; }

    public int AuthorId { get; set; }

    // Navigation property. Needs to be loaded explicitly.
    public AppUser? Author { get; set; }
}