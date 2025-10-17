using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelListing.Api.Domain.Migrations
{
    /// <inheritdoc />
    public partial class RemoveNameIndexHotelsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Hotels_Name",
                table: "Hotels");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Hotels",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
        }
    }
}
