using System.Diagnostics.CodeAnalysis;

namespace MiniTwit.Models;

public class TimelineViewModel
{
    [MemberNotNullWhen(true, nameof(UserId))]
    [MemberNotNullWhen(true, nameof(Username))]
    public bool IsUserTimeline => UserId.HasValue;

    public int? UserId { get; set; }

    public string? Username { get; set; }

    public bool FollowsUser { get; set; }

    public required List<MessageViewModel> Messages { get; set; }
}