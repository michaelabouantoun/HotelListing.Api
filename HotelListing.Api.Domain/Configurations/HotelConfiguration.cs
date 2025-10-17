using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelListing.Api.Domain.Configurations;

public class HotelConfiguration : IEntityTypeConfiguration<Hotel>
{
    public void Configure(EntityTypeBuilder<Hotel> builder)
    {
        builder.HasIndex(h => h.PerNightRate);
        builder.HasIndex(h => new { h.CountryId, h.PerNightRate });
        builder.HasIndex(h => new { h.CountryId, h.Rating });
        builder.HasIndex(h => h.CountryId);
    }
}
