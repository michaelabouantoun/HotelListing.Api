using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelListing.Api.Domain.Migrations
{
    /// <inheritdoc />
    public partial class RefactorBookingIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookings_HotelId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_UserId",
                table: "Bookings");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_HotelId_CheckIn",
                table: "Bookings",
                columns: new[] { "HotelId", "CheckIn" });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_HotelId_TotalPrice",
                table: "Bookings",
                columns: new[] { "HotelId", "TotalPrice" });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_UserId_HotelId",
                table: "Bookings",
                columns: new[] { "UserId", "HotelId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookings_HotelId_CheckIn",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_HotelId_TotalPrice",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_UserId_HotelId",
                table: "Bookings");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_HotelId",
                table: "Bookings",
                column: "HotelId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_UserId",
                table: "Bookings",
                column: "UserId");
        }
    }
}
