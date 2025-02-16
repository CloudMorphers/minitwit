namespace MiniTwit.Models;

public class MessageViewModel
{
    public required string Email { get; init; }

    public required string Username { get; init; }

    public required string Text { get; init; }

    public required DateTime PublishDate { get; init; }
}