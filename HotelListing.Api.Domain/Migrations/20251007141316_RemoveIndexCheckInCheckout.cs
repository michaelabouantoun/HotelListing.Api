using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelListing.Api.Domain.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIndexCheckInCheckout : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookings_CheckIn_CheckOut",
                table: "Bookings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Bookings_CheckIn_CheckOut",
                table: "Bookings",
                columns: new[] { "CheckIn", "CheckOut" });
        }
    }
}
