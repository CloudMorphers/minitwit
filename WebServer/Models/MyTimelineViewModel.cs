using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MiniTwit.Models;

public class MyTimelineViewModel
{
    [Required(ErrorMessage = "You have to enter a message")]
    public string Text { get; set; } = string.Empty;

    [BindNever]
    public List<MessageViewModel>? Messages { get; set; }
}
