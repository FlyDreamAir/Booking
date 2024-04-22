using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlyDreamAir.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAddOns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "AddOns",
                type: "character varying(8)",
                maxLength: 8,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(5)",
                oldMaxLength: 5);

            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                table: "AddOns",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "AddOns",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DishName",
                table: "AddOns",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageSrc",
                table: "AddOns",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Meal_ImageSrc",
                table: "AddOns",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Amount",
                table: "AddOns");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "AddOns");

            migrationBuilder.DropColumn(
                name: "DishName",
                table: "AddOns");

            migrationBuilder.DropColumn(
                name: "ImageSrc",
                table: "AddOns");

            migrationBuilder.DropColumn(
                name: "Meal_ImageSrc",
                table: "AddOns");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "AddOns",
                type: "character varying(5)",
                maxLength: 5,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(8)",
                oldMaxLength: 8);
        }
    }
}
