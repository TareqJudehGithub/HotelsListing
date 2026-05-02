using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HotelListingAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddeddefaultRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "B0755520-E5ED-42D4-97D4-AE078CB8D35A", "B0755520-E5ED-42D4-97D4-AE078CB8D35A", "User", "USER" },
                    { "DD0491E3-A9BD-402F-A364-D1EFDAB4DF6A", "DD0491E3-A9BD-402F-A364-D1EFDAB4DF6A", "Administrator", "ADMINISTRATOR" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "B0755520-E5ED-42D4-97D4-AE078CB8D35A");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "DD0491E3-A9BD-402F-A364-D1EFDAB4DF6A");
        }
    }
}
