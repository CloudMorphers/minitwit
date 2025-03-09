using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MiniTwit.Models.Api;

public class RegisterInputModel
{
    [Required(ErrorMessage = "You have to enter a username")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "You have to enter an email address")]
    [EmailAddress(ErrorMessage = "You have to enter a valid email address")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("pwd")]
    [Required(ErrorMessage = "You have to enter a password")]
    public string Password { get; set; } = string.Empty;
}
