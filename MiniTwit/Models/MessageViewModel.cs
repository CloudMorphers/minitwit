namespace MiniTwit.Models;

public class MessageViewModel
{
    public int Id { get; set; }  // ✅ Primary key for Entity Framework

    public required string Username { get; set; }

    public required string Text { get; set; } // ✅ Add this to match `_Messages.cshtml`

    public required DateTime PublishDate { get; set; } // ✅ Add this to match `_Messages.cshtml`
}