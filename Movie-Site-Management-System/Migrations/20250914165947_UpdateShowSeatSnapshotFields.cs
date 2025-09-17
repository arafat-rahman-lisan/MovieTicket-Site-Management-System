using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Movie_Site_Management_System.Migrations
{
    /// <inheritdoc />
    public partial class UpdateShowSeatSnapshotFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Drop FKs/PK that block schema changes
            migrationBuilder.DropForeignKey(
                name: "FK_ShowSeats_Seats_SeatId",
                table: "ShowSeats");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ShowSeats",
                table: "ShowSeats");

            // 2) Remove legacy columns if present
            migrationBuilder.DropColumn(
                name: "HoldExpiresAt",
                table: "ShowSeats");

            migrationBuilder.DropColumn(
                name: "PriceAtBooking",
                table: "ShowSeats");

            // 3) Add new columns (SeatTypeId as NULLABLE first; NO default 0)
            migrationBuilder.AddColumn<long>(
                name: "ShowSeatId",
                table: "ShowSeats",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "ShowSeats",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "ShowSeats",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "SeatTypeId",
                table: "ShowSeats",
                type: "smallint",
                nullable: true); // nullable first

            // 4) New PK and helpful indexes
            migrationBuilder.AddPrimaryKey(
                name: "PK_ShowSeats",
                table: "ShowSeats",
                column: "ShowSeatId");

            migrationBuilder.CreateIndex(
                name: "IX_ShowSeats_ShowId_SeatId",
                table: "ShowSeats",
                columns: new[] { "ShowId", "SeatId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShowSeats_SeatTypeId",
                table: "ShowSeats",
                column: "SeatTypeId");

            // 5) BACKFILL data to satisfy future NOT NULL + FK constraints
            //    a) SeatTypeId <- Seats.SeatTypeId
            migrationBuilder.Sql(@"
UPDATE ss
SET ss.SeatTypeId = s.SeatTypeId
FROM dbo.ShowSeats ss
JOIN dbo.Seats s ON s.SeatId = ss.SeatId
WHERE ss.SeatTypeId IS NULL OR ss.SeatTypeId = 0;
");

            //    b) Fallback SeatTypeId (in case any still NULL)
            migrationBuilder.Sql(@"
DECLARE @fallback SMALLINT = (SELECT TOP 1 SeatTypeId FROM dbo.SeatTypes ORDER BY SeatTypeId);
UPDATE dbo.ShowSeats SET SeatTypeId = @fallback WHERE SeatTypeId IS NULL;
");

            //    c) Price snapshot <- SeatTypes.BasePrice
            migrationBuilder.Sql(@"
UPDATE ss
SET ss.Price = st.BasePrice
FROM dbo.ShowSeats ss
JOIN dbo.SeatTypes st ON st.SeatTypeId = ss.SeatTypeId
WHERE ss.Price = 0;
");

            // 6) Enforce NOT NULL now that data is consistent
            migrationBuilder.AlterColumn<short>(
                name: "SeatTypeId",
                table: "ShowSeats",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint",
                oldNullable: true);

            // 7) Re-add FKs (now safe)
            migrationBuilder.AddForeignKey(
                name: "FK_ShowSeats_SeatTypes_SeatTypeId",
                table: "ShowSeats",
                column: "SeatTypeId",
                principalTable: "SeatTypes",
                principalColumn: "SeatTypeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ShowSeats_Seats_SeatId",
                table: "ShowSeats",
                column: "SeatId",
                principalTable: "Seats",
                principalColumn: "SeatId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShowSeats_SeatTypes_SeatTypeId",
                table: "ShowSeats");

            migrationBuilder.DropForeignKey(
                name: "FK_ShowSeats_Seats_SeatId",
                table: "ShowSeats");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ShowSeats",
                table: "ShowSeats");

            migrationBuilder.DropIndex(
                name: "IX_ShowSeats_SeatTypeId",
                table: "ShowSeats");

            migrationBuilder.DropIndex(
                name: "IX_ShowSeats_ShowId_SeatId",
                table: "ShowSeats");

            migrationBuilder.DropColumn(
                name: "ShowSeatId",
                table: "ShowSeats");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "ShowSeats");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "ShowSeats");

            migrationBuilder.DropColumn(
                name: "SeatTypeId",
                table: "ShowSeats");

            migrationBuilder.AddColumn<DateTime>(
                name: "HoldExpiresAt",
                table: "ShowSeats",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PriceAtBooking",
                table: "ShowSeats",
                type: "decimal(10,2)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ShowSeats",
                table: "ShowSeats",
                columns: new[] { "ShowId", "SeatId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ShowSeats_Seats_SeatId",
                table: "ShowSeats",
                column: "SeatId",
                principalTable: "Seats",
                principalColumn: "SeatId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
