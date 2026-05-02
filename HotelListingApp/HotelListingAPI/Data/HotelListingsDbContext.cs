using HotelListing.Api.Data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace HotelListingAPI.Data
{
    public class HotelListingsDbContext : IdentityDbContext<ApplicationUser>
    {
        #region Constructors
        public HotelListingsDbContext(DbContextOptions<HotelListingsDbContext> options) : base(options)
        {
        }
        #endregion

        #region DbSets
        public DbSet<Hotel> Hotels { get; set; }
        public DbSet<Country> Countries { get; set; }

        public DbSet<ApiKey> ApiKeys { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Default builder
            base.OnModelCreating(builder);

            // Custom builder - API Auth Key
            builder.Entity<ApiKey>(b =>
            {
                b.HasIndex(k => k.Key).IsUnique();
            });

            // Apply configuration from the assembly
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
        #endregion
    }
}
