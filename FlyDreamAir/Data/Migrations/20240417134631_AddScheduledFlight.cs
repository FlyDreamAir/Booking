using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlyDreamAir.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddScheduledFlight : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Flights_FlightId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_FlightId",
                table: "Tickets");

            migrationBuilder.RenameColumn(
                name: "DepartureTime",
                table: "Tickets",
                newName: "FlightDepartureTime");

            migrationBuilder.CreateTable(
                name: "ScheduledFlights",
                columns: table => new
                {
                    DepartureTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FlightId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledFlights", x => new { x.FlightId, x.DepartureTime });
                    table.ForeignKey(
                        name: "FK_ScheduledFlights_Flights_FlightId",
                        column: x => x.FlightId,
                        principalTable: "Flights",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_FlightId_FlightDepartureTime",
                table: "Tickets",
                columns: new[] { "FlightId", "FlightDepartureTime" });

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_ScheduledFlights_FlightId_FlightDepartureTime",
                table: "Tickets",
                columns: new[] { "FlightId", "FlightDepartureTime" },
                principalTable: "ScheduledFlights",
                principalColumns: new[] { "FlightId", "DepartureTime" },
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_ScheduledFlights_FlightId_FlightDepartureTime",
                table: "Tickets");

            migrationBuilder.DropTable(
                name: "ScheduledFlights");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_FlightId_FlightDepartureTime",
                table: "Tickets");

            migrationBuilder.RenameColumn(
                name: "FlightDepartureTime",
                table: "Tickets",
                newName: "DepartureTime");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_FlightId",
                table: "Tickets",
                column: "FlightId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Flights_FlightId",
                table: "Tickets",
                column: "FlightId",
                principalTable: "Flights",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
