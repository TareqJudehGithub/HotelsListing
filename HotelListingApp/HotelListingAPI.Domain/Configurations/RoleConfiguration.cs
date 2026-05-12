using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using HotelListingAPI.Common.Constants;

namespace HotelListingAPI.Domain.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<IdentityRole>
{
    public void Configure(EntityTypeBuilder<IdentityRole> builder)
    {
        // Role Ids
        var adminRoleId = "DD0491E3-A9BD-402F-A364-D1EFDAB4DF6A";
        var userRoleId = "B0755520-E5ED-42D4-97D4-AE078CB8D35A";
        var hotelAdminId = "591d71d8-cbd8-4336-befb-17b9012eb9c7";

        builder.HasData(
            // Admin
            new IdentityRole
            {
                Id = adminRoleId,
                Name = RoleNames.Administrator,
                NormalizedName = RoleNames.Administrator.ToUpper(),
                ConcurrencyStamp = adminRoleId
            },
            // User

            new IdentityRole
            {
                Id = userRoleId,
                Name = RoleNames.User,
                NormalizedName = RoleNames.User.ToUpper(),
                ConcurrencyStamp = userRoleId
            },
            new IdentityRole
            {
                Id = hotelAdminId,
                Name = RoleNames.HotelAdmin,
                NormalizedName = RoleNames.HotelAdmin.ToUpper(),
                ConcurrencyStamp = hotelAdminId
            }
            );
    }
}
