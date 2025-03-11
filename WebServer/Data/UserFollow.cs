namespace MiniTwit.Data;

public class UserFollow
{
    public int FollowerId { get; set; }

    public AppUser? Follower { get; set; }

    public int FollowingId { get; set; }

    public AppUser? Following { get; set; }
}
