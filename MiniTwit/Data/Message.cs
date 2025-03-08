namespace MiniTwit.Data;

public class Message
{
    public int Id { get; set; }

    public string Text { get; set; } = null!;

    public DateTime PublishDate { get; set; }

    public bool IsFlagged { get; set; }

    public int AuthorId { get; set; }

    public AppUser? Author { get; set; }
}
