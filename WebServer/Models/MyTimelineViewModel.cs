using System.ComponentModel.DataAnnotations;

namespace MiniTwit.Models;

public class MyTimelineViewModel
{
    [Required]
    public string Text { get; set; } = string.Empty;

    public required List<MessageViewModel> Messages { get; set; }
}