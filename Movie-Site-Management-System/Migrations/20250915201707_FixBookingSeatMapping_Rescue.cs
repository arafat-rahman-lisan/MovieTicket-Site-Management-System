using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Movie_Site_Management_System.Migrations
{
    public partial class FixBookingSeatMapping_Rescue : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 0) If an FK was partially added in some environments, drop it first (safe if it doesn't exist)
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_BookingSeats_ShowSeats_ShowSeatId')
    ALTER TABLE [BookingSeats] DROP CONSTRAINT [FK_BookingSeats_ShowSeats_ShowSeatId];
");

            // 1) Drop DEFAULT constraint on BookingSeats.ShowSeatId (we don't know its auto name)
            migrationBuilder.Sql(@"
DECLARE @df NVARCHAR(128);
SELECT @df = dc.name
FROM sys.default_constraints AS dc
INNER JOIN sys.columns AS c
    ON c.default_object_id = dc.object_id
INNER JOIN sys.tables AS t
    ON t.object_id = dc.parent_object_id
WHERE t.name = 'BookingSeats' AND c.name = 'ShowSeatId';
IF @df IS NOT NULL
    EXEC(N'ALTER TABLE [BookingSeats] DROP CONSTRAINT [' + @df + ']');
");

            // 2) Make column nullable so we can clean/patch data
            migrationBuilder.AlterColumn<long>(
                name: "ShowSeatId",
                table: "BookingSeats",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            // 3) Convert 0s to NULL (from earlier NOT NULL DEFAULT 0)
            migrationBuilder.Sql(@"UPDATE [BookingSeats] SET [ShowSeatId] = NULL WHERE [ShowSeatId] = 0;");

            // 4) Backfill ShowSeatId by joining BookingSeats -> Bookings (ShowId) -> ShowSeats (ShowId+SeatId)
            migrationBuilder.Sql(@"
UPDATE bs
SET bs.ShowSeatId = ss.ShowSeatId
FROM BookingSeats bs
INNER JOIN Bookings b   ON b.BookingId = bs.BookingId
INNER JOIN ShowSeats ss ON ss.ShowId = b.ShowId AND ss.SeatId = bs.SeatId
WHERE bs.ShowSeatId IS NULL;
");

            // 5) Add the FK (nullable FK is fine)
            migrationBuilder.AddForeignKey(
                name: "FK_BookingSeats_ShowSeats_ShowSeatId",
                table: "BookingSeats",
                column: "ShowSeatId",
                principalTable: "ShowSeats",
                principalColumn: "ShowSeatId",
                onDelete: ReferentialAction.Restrict
            );

            // (Optional) enforce NOT NULL after verifying no NULL remain:
            // migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM BookingSeats WHERE ShowSeatId IS NULL) RAISERROR ('Null ShowSeatId rows remain', 16, 1);");
            // migrationBuilder.AlterColumn<long>(
            //     name: "ShowSeatId",
            //     table: "BookingSeats",
            //     type: "bigint",
            //     nullable: false,
            //     oldClrType: typeof(long),
            //     oldType: "bigint",
            //     oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop FK if present
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_BookingSeats_ShowSeats_ShowSeatId')
    ALTER TABLE [BookingSeats] DROP CONSTRAINT [FK_BookingSeats_ShowSeats_ShowSeatId];
");

            // Revert to NOT NULL with default 0 (recreates the original bad state)
            migrationBuilder.AlterColumn<long>(
                name: "ShowSeatId",
                table: "BookingSeats",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);
        }
    }
}
