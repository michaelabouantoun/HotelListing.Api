using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelListing.Api.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddHotelIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Hotels",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Hotels_Name",
                table: "Hotels",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Hotels_PerNightRate",
                table: "Hotels",
                column: "PerNightRate");

            migrationBuilder.CreateIndex(
                name: "IX_Hotels_Rating",
                table: "Hotels",
                column: "Rating");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Hotels_Name",
                table: "Hotels");

            migrationBuilder.DropIndex(
                name: "IX_Hotels_PerNightRate",
                table: "Hotels");

            migrationBuilder.DropIndex(
                name: "IX_Hotels_Rating",
                table: "Hotels");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Hotels",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
