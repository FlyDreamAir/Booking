using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlyDreamAir.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingCancellationId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CancellationId",
                table: "Bookings",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_CancellationId",
                table: "Bookings",
                column: "CancellationId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookings_CancellationId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "CancellationId",
                table: "Bookings");
        }
    }
}
