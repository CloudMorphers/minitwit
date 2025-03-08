using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MiniTwit.Data
{
    public class AppDbContext : IdentityDbContext<AppUser, IdentityRole<int>, int>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<Message> Messages { get; set; }

        public DbSet<UserFollow> UserFollows { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder
                .Entity<UserFollow>()
                .HasKey(userFollow => new { userFollow.FollowerId, userFollow.FollowingId });
            modelBuilder
                .Entity<UserFollow>()
                .HasOne(userFollow => userFollow.Follower)
                .WithMany(u => u.Following)
                .HasForeignKey(userFollow => userFollow.FollowerId);
            modelBuilder
                .Entity<UserFollow>()
                .HasOne(userFollow => userFollow.Following)
                .WithMany(u => u.Followers)
                .HasForeignKey(userFollow => userFollow.FollowingId);
        }
    }
}
