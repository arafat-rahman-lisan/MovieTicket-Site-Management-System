using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Movie_Site_Management_System.Migrations
{
    /// <inheritdoc />
    public partial class HallSlotDeleteRestrict : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        { 
            migrationBuilder.DropForeignKey(
                name: "FK_HallSlots_Halls_HallId",
                table: "HallSlots");

            migrationBuilder.AddForeignKey(
                name: "FK_HallSlots_Halls_HallId",
                table: "HallSlots",
                column: "HallId",  
                principalTable: "Halls",
                principalColumn: "HallId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HallSlots_Halls_HallId",
                table: "HallSlots");

            migrationBuilder.AddForeignKey(
                name: "FK_HallSlots_Halls_HallId",
                table: "HallSlots",
                column: "HallId",
                principalTable: "Halls",
                principalColumn: "HallId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
