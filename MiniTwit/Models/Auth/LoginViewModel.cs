using System.ComponentModel.DataAnnotations;

namespace MiniTwit.Models.Auth;

public class LoginViewModel
{
    [Required(ErrorMessage = "You have to enter a username")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "You have to enter a password")]
    public string Password { get; set; } = string.Empty;
}
