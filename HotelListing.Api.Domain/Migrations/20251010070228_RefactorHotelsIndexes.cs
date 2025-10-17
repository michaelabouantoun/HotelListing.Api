using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelListing.Api.Domain.Migrations
{
    /// <inheritdoc />
    public partial class RefactorHotelsIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Hotels_CountryId",
                table: "Hotels");

            migrationBuilder.DropIndex(
                name: "IX_Hotels_Rating",
                table: "Hotels");

            migrationBuilder.CreateIndex(
                name: "IX_Hotels_CountryId_PerNightRate",
                table: "Hotels",
                columns: new[] { "CountryId", "PerNightRate" });

            migrationBuilder.CreateIndex(
                name: "IX_Hotels_CountryId_Rating",
                table: "Hotels",
                columns: new[] { "CountryId", "Rating" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Hotels_CountryId_PerNightRate",
                table: "Hotels");

            migrationBuilder.DropIndex(
                name: "IX_Hotels_CountryId_Rating",
                table: "Hotels");

            migrationBuilder.CreateIndex(
                name: "IX_Hotels_CountryId",
                table: "Hotels",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Hotels_Rating",
                table: "Hotels",
                column: "Rating");
        }
    }
}
