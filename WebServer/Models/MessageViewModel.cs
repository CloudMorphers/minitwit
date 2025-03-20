using System.Security.Cryptography;
using System.Text;

namespace MiniTwit.Models;

public class MessageViewModel
{
    public required string Email { get; init; }

    public required string Username { get; init; }

    public required string Text { get; init; }

    public required DateTime PublishDate { get; init; }

    public string GetGravatarUrl(int size = 48)
    {
        var normalizedEmail = Email.Trim().ToLowerInvariant();
        var emailBytes = Encoding.UTF8.GetBytes(normalizedEmail);
        var hashBytes = MD5.HashData(emailBytes);
        var hashHex = Convert.ToHexStringLower(hashBytes);
        var url = $"http://www.gravatar.com/avatar/{hashHex}?d=identicon&s={size}";
        return url;
    }
}
