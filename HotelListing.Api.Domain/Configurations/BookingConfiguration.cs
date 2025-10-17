﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelListing.Api.Domain.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.Property(q => q.Status)
            .HasConversion<string>()
            .HasMaxLength(20);
        builder.HasIndex(x => new { x.UserId,x.HotelId });
        builder.HasIndex(x => new { x.HotelId, x.CheckIn });
        builder.HasIndex(x => new { x.HotelId, x.TotalPrice });
    }
}
