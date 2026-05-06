using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelListingAPI.Data.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        // Convert BookingStatus numbers into their string values

        // This will store the enum as a string in the database, which is more readable and maintainable.
        builder.Property(q => q.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        // Indexes for optimizing queries
        builder.HasIndex(q => q.UserId);
        builder.HasIndex(q => q.HotelId);

        // builder.HasIndex(q => q.CheckIn);
        // builder.HasIndex(q => q.CheckOut);

        // Composite index
        builder.HasIndex(q => new { q.CheckIn, q.CheckOut });

    }

}
