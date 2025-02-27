namespace MiniTwit.Models
{
    public class Follower
    {
        public int WhoId { get; set; }  // User who is following
        public int WhomId { get; set; } // User being followed
    }
}
