using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Movie_Site_Management_System.Migrations
{
    /// <summary>
    /// Backfill guard: NO schema churn.
    /// - Does NOT alter nullability (first migration already set NOT NULL)
    /// - Does NOT add FK/Index if they already exist
    /// - Safe to run after the primary FixBookingSeatMapping
    /// </summary>
    public partial class FixBookingSeatMapping_Backfill : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Ensure FK NOT duplicated (skip if exists)
            migrationBuilder.Sql(@"
IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = N'FK_BookingSeats_ShowSeats_ShowSeatId'
      AND parent_object_id = OBJECT_ID(N'dbo.BookingSeats')
)
BEGIN
    ALTER TABLE dbo.BookingSeats
    ADD CONSTRAINT FK_BookingSeats_ShowSeats_ShowSeatId
        FOREIGN KEY (ShowSeatId) REFERENCES dbo.ShowSeats (ShowSeatId) ON DELETE NO ACTION;
END
");

            // 2) Ensure index NOT duplicated (skip if exists)
            migrationBuilder.Sql(@"
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_BookingSeats_ShowSeatId'
      AND object_id = OBJECT_ID(N'dbo.BookingSeats')
)
BEGIN
    CREATE INDEX [IX_BookingSeats_ShowSeatId] ON [dbo].[BookingSeats]([ShowSeatId]);
END
");

            // 3) Optional (no-op here): If you intended extra data fixes, put ONLY data updates here.
            //    Do NOT ALTER COLUMN here—first migration already finalized schema.
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Make this reversible but safe (only drop if present)

            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = N'FK_BookingSeats_ShowSeats_ShowSeatId'
      AND parent_object_id = OBJECT_ID(N'dbo.BookingSeats')
)
BEGIN
    ALTER TABLE dbo.BookingSeats DROP CONSTRAINT FK_BookingSeats_ShowSeats_ShowSeatId;
END
");

            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_BookingSeats_ShowSeatId'
      AND object_id = OBJECT_ID(N'dbo.BookingSeats')
)
BEGIN
    DROP INDEX [IX_BookingSeats_ShowSeatId] ON [dbo].[BookingSeats];
END
");
        }
    }
}
