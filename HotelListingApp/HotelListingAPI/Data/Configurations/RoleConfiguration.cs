using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelListingAPI.Data.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<IdentityRole>
{
    public void Configure(EntityTypeBuilder<IdentityRole> builder)
    {
        // Role Ids
        var adminRoleId = "DD0491E3-A9BD-402F-A364-D1EFDAB4DF6A";
        var userRoleId = "B0755520-E5ED-42D4-97D4-AE078CB8D35A";

        builder.HasData(
            // Admin
            new IdentityRole
            {
                Id = adminRoleId,
                Name = "Administrator",
                NormalizedName = "ADMINISTRATOR",
                ConcurrencyStamp = adminRoleId
            },
            // User

            new IdentityRole
            {
                Id = userRoleId,
                Name = "User",
                NormalizedName = "USER",
                ConcurrencyStamp = userRoleId
            }
            );
    }
}
