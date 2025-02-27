namespace MiniTwit.Models
{
    public class Message
    {
        public int MessageId { get; set; }
        public int AuthorId { get; set; }  // The user who authored the message
        public string Text { get; set; }   // The message content
        public long PubDate { get; set; }  // Publication date (epoch timestamp)
        public int Flagged { get; set; }   // Flag status (e.g., 0 = normal, 1 = flagged)
    }
}
