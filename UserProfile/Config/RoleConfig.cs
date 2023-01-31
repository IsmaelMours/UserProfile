using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace UserProfile.Config
{
    public class RoleConfig : IEntityTypeConfiguration<IdentityRole>
    {
        public void Configure(EntityTypeBuilder<IdentityRole> builder) => _ = builder.HasData(
                new IdentityRole
                {
                    Name = "Admin",
                  
                }, new IdentityRole
                {
                    Name = "User",
                   
                }
            );
    }
}
