using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Movie_Site_Management_System.Migrations
{
    /// <inheritdoc />
    public partial class AlignBookingSeatRequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 0) Drop FK & index if they exist so we can safely alter the column
            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = N'FK_BookingSeats_ShowSeats_ShowSeatId'
      AND parent_object_id = OBJECT_ID(N'dbo.BookingSeats')
)
BEGIN
    ALTER TABLE dbo.BookingSeats DROP CONSTRAINT [FK_BookingSeats_ShowSeats_ShowSeatId];
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

            // 1) Backfill any remaining NULLs from (BookingId -> Bookings.ShowId) + SeatId -> ShowSeats(ShowId,SeatId)
            migrationBuilder.Sql(@"
UPDATE bs
SET bs.ShowSeatId = ss.ShowSeatId
FROM dbo.BookingSeats bs
JOIN dbo.Bookings b   ON b.BookingId = bs.BookingId
JOIN dbo.ShowSeats ss ON ss.ShowId   = b.ShowId
                     AND ss.SeatId   = bs.SeatId
WHERE bs.ShowSeatId IS NULL;
");

            // 2) As a safety net, delete any stubborn rows that still couldn't map (avoids NOT NULL failure)
            migrationBuilder.Sql(@"DELETE FROM dbo.BookingSeats WHERE ShowSeatId IS NULL;");

            // 3) Enforce NOT NULL now that data is clean
            migrationBuilder.Sql(@"ALTER TABLE dbo.BookingSeats ALTER COLUMN ShowSeatId bigint NOT NULL;");

            // 4) Recreate index and FK (idempotent)
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

            migrationBuilder.Sql(@"
IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = N'FK_BookingSeats_ShowSeats_ShowSeatId'
      AND parent_object_id = OBJECT_ID(N'dbo.BookingSeats')
)
BEGIN
    ALTER TABLE dbo.BookingSeats
    ADD CONSTRAINT [FK_BookingSeats_ShowSeats_ShowSeatId]
        FOREIGN KEY ([ShowSeatId]) REFERENCES [dbo].[ShowSeats] ([ShowSeatId]) ON DELETE NO ACTION;
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop FK & index then relax column back to nullable
            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = N'FK_BookingSeats_ShowSeats_ShowSeatId'
      AND parent_object_id = OBJECT_ID(N'dbo.BookingSeats')
)
BEGIN
    ALTER TABLE dbo.BookingSeats DROP CONSTRAINT [FK_BookingSeats_ShowSeats_ShowSeatId];
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

            migrationBuilder.Sql(@"ALTER TABLE dbo.BookingSeats ALTER COLUMN ShowSeatId bigint NULL;");
        }
    }
}
