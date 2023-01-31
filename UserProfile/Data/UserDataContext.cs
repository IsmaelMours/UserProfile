
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.EntityFrameworkCore;
using UserProfile.Model;
using UserProfile.Config;
namespace UserProfile.Data
{
    public class UserDataContext : DbContext
    {
        public  UserDataContext (DbContextOptions<UserDataContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Image> Images { get; set; }



        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfiguration(new RoleConfig());
        }
    }
}
