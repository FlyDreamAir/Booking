using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlyDreamAir.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSeat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "FlightId",
                table: "AddOns",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEmergencyRow",
                table: "AddOns",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<char>(
                name: "SeatPosition",
                table: "AddOns",
                type: "character(1)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SeatRow",
                table: "AddOns",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SeatType",
                table: "AddOns",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AddOns_FlightId",
                table: "AddOns",
                column: "FlightId");

            migrationBuilder.AddForeignKey(
                name: "FK_AddOns_Flights_FlightId",
                table: "AddOns",
                column: "FlightId",
                principalTable: "Flights",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AddOns_Flights_FlightId",
                table: "AddOns");

            migrationBuilder.DropIndex(
                name: "IX_AddOns_FlightId",
                table: "AddOns");

            migrationBuilder.DropColumn(
                name: "FlightId",
                table: "AddOns");

            migrationBuilder.DropColumn(
                name: "IsEmergencyRow",
                table: "AddOns");

            migrationBuilder.DropColumn(
                name: "SeatPosition",
                table: "AddOns");

            migrationBuilder.DropColumn(
                name: "SeatRow",
                table: "AddOns");

            migrationBuilder.DropColumn(
                name: "SeatType",
                table: "AddOns");
        }
    }
}
