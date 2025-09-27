using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection; //need to include it

namespace HotelListing.Api.Domain;

public class HotelListingDbContext : IdentityDbContext<ApplicationUser>
{
    public HotelListingDbContext(DbContextOptions<HotelListingDbContext> options) : base(options)
    {

    }
    public DbSet<Country> Countries { get; set; }
    public DbSet<Hotel> Hotels { get; set; }
    public DbSet<ApiKey> ApiKey { get; set; }
    public DbSet<HotelAdmin> HotelAdmins { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<ApiKey>(b =>
        {
            b.HasIndex(k => k.Key).IsUnique();
        });
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

}
