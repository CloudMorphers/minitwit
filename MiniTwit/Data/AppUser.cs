using Microsoft.AspNetCore.Identity;

namespace MiniTwit.Data;

public class AppUser : IdentityUser<int>
{
    public ICollection<Message> Messages { get; set; } = [];

    public ICollection<UserFollow> Following { get; set; } = [];

    public ICollection<UserFollow> Followers { get; set; } = [];
}
