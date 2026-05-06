using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelListingAPI.Migrations
{
    /// <inheritdoc />
    public partial class MergedCheckInandCheckOutindexsAddedannotationTypeNamedecimal183forPerNightRateandforTotalPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookings_CheckIn",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_CheckOut",
                table: "Bookings");

            migrationBuilder.AlterColumn<decimal>(
                name: "PerNightRate",
                schema: "dbo",
                table: "Hotels",
                type: "decimal(18,3)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalPrice",
                table: "Bookings",
                type: "decimal(18,3)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_CheckIn_CheckOut",
                table: "Bookings",
                columns: new[] { "CheckIn", "CheckOut" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookings_CheckIn_CheckOut",
                table: "Bookings");

            migrationBuilder.AlterColumn<decimal>(
                name: "PerNightRate",
                schema: "dbo",
                table: "Hotels",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,3)");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalPrice",
                table: "Bookings",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,3)");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_CheckIn",
                table: "Bookings",
                column: "CheckIn");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_CheckOut",
                table: "Bookings",
                column: "CheckOut");
        }
    }
}
