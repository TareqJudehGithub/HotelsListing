using Microsoft.EntityFrameworkCore;

namespace HotelListingAPI.Data
{
    public class HotelListingsDbContext : DbContext
    {
        #region Constructors
        public HotelListingsDbContext(DbContextOptions<HotelListingsDbContext> options) : base(options)
        {
        }
        #endregion

        #region DbSets
        public DbSet<Hotel> Hotels { get; set; }
        public DbSet<Country> Countries { get; set; }
        #endregion
    }
}
