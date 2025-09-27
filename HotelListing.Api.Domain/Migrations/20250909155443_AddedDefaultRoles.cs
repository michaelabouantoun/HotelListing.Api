using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HotelListing.Api.Domain.Migrations;
/// <inheritdoc />
public partial class AddedDefaultRoles : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.InsertData(
            table: "AspNetRoles",
            columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
            values: new object[,]
            {
                    { "0242b8ab-2298-4df8-9a29-7838a14b291e", null, "Administrator", "ADMINISTRATOR" },
                    { "7ef506a7-ec48-40e9-b10e-9d11edcde10c", null, "User", "USER" }
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DeleteData(
            table: "AspNetRoles",
            keyColumn: "Id",
            keyValue: "0242b8ab-2298-4df8-9a29-7838a14b291e");

        migrationBuilder.DeleteData(
            table: "AspNetRoles",
            keyColumn: "Id",
            keyValue: "7ef506a7-ec48-40e9-b10e-9d11edcde10c");
    }
}

