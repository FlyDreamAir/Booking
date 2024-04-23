using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlyDreamAir.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateMissingColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PassportId",
                table: "Customers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "From",
                table: "Bookings",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "To",
                table: "Bookings",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PassportId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "From",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "To",
                table: "Bookings");
        }
    }
}
