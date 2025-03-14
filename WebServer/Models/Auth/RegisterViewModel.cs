using System.ComponentModel.DataAnnotations;

namespace MiniTwit.Models.Auth;

public class RegisterViewModel
{
    [Required(ErrorMessage = "You have to enter a username")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "You have to enter an email address")]
    [EmailAddress(ErrorMessage = "You have to enter a valid email address")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "You have to enter a password")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "You have to enter a password confirmation")]
    [Compare(nameof(Password), ErrorMessage = "The two passwords do not match")]
    public string PasswordConfirmation { get; set; } = string.Empty;
}
