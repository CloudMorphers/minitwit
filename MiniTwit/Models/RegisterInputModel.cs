using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MiniTwit.Models;

public class RegisterInputModel
{
    [Required(ErrorMessage = "You have to enter a username")]
    public string Username { get; set; }

    [EmailAddress(ErrorMessage = "You have to enter a valid email address")]
    public string Email { get; set; }

    [JsonPropertyName("pwd")]
    [Required(ErrorMessage = "You have to enter a password")]
    public string Password { get; set; }
}
