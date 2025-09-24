using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelListing.Api.Data.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<IdentityRole>
{
    public void Configure(EntityTypeBuilder<IdentityRole> builder)
    {
        builder.HasData(
            new IdentityRole
            {
                Id = "0242b8ab-2298-4df8-9a29-7838a14b291e",
                Name = "Administrator",
                NormalizedName = "ADMINISTRATOR"
            },
             new IdentityRole
             {
                 Id = "7ef506a7-ec48-40e9-b10e-9d11edcde10c",
                 Name = "User",
                 NormalizedName = "USER"
             },
             new IdentityRole
             {
                 Id = "7ef506a7-2298-40e9-b10e-7838a14b291e",
                 Name = "Hotel Admin",
                 NormalizedName = "HOTEL ADMIN"
             }
            );
    }
}
