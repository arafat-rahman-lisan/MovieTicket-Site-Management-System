using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Movie_Site_Management_System.Migrations
{
    /// <inheritdoc />
    public partial class AddHoldUntilToShowSeat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "HoldUntil",
                table: "ShowSeats",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HoldUntil",
                table: "ShowSeats");
        }
    }
}
