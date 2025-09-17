using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Movie_Site_Management_System.Migrations
{
    /// <summary>
    /// Safe mapping BookingSeats -> ShowSeats:
    /// Order:
    ///  1) Ensure Shows.Date
    ///  2) Ensure BookingSeats.ShowSeatId exists & is NULLABLE (drop default)
    ///  3) Backfill ShowSeatId from (BookingId -> Bookings.ShowId) + SeatId -> ShowSeats(ShowId,SeatId)
    ///  4) Delete orphans (optional; comment to inspect instead)
    ///  5) DROP FK & INDEX (if exist), then ALTER to NOT NULL
    ///  6) Recreate INDEX and then FK
    /// </summary>
    public partial class FixBookingSeatMapping : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // (A) Ensure Shows.Date exists
            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.Shows', 'Date') IS NULL
BEGIN
    ALTER TABLE dbo.Shows ADD [Date] date NOT NULL DEFAULT ('0001-01-01');
END
");

            // (B) Ensure ShowSeatId column exists and is nullable; drop any default constraint
            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.BookingSeats', 'ShowSeatId') IS NULL
BEGIN
    ALTER TABLE dbo.BookingSeats ADD ShowSeatId bigint NULL;
END
ELSE
BEGIN
    -- drop default constraint if any
    DECLARE @df NVARCHAR(128);
    SELECT @df = dc.name
    FROM sys.default_constraints dc
    JOIN sys.columns c
      ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
    WHERE dc.parent_object_id = OBJECT_ID(N'dbo.BookingSeats')
      AND c.name = N'ShowSeatId';

    IF @df IS NOT NULL
    BEGIN
        DECLARE @sql NVARCHAR(MAX) = N'ALTER TABLE dbo.BookingSeats DROP CONSTRAINT [' + @df + N']';
        EXEC sp_executesql @sql;
    END

    -- ensure nullable for backfill
    ALTER TABLE dbo.BookingSeats ALTER COLUMN ShowSeatId bigint NULL;
END
");

            // (C) Backfill ShowSeatId via (BookingId -> Bookings.ShowId) + SeatId -> ShowSeats(ShowId,SeatId)
            migrationBuilder.Sql(@"
UPDATE bs
SET bs.ShowSeatId = ss.ShowSeatId
FROM dbo.BookingSeats bs
JOIN dbo.Bookings b   ON b.BookingId = bs.BookingId
JOIN dbo.ShowSeats ss ON ss.ShowId   = b.ShowId
                     AND ss.SeatId   = bs.SeatId;
");

            // Clear legacy zeros → NULL
            migrationBuilder.Sql(@"UPDATE dbo.BookingSeats SET ShowSeatId = NULL WHERE ShowSeatId = 0;");

            // STRICT MODE: remove rows that still couldn't map (comment to inspect instead)
            migrationBuilder.Sql(@"DELETE FROM dbo.BookingSeats WHERE ShowSeatId IS NULL;");

            // (D) Drop FK & INDEX if they exist so we can ALTER the column to NOT NULL
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

            // (E) Now enforce NOT NULL
            migrationBuilder.Sql(@"ALTER TABLE dbo.BookingSeats ALTER COLUMN ShowSeatId bigint NOT NULL;");

            // (F) Recreate INDEX
            migrationBuilder.Sql(@"
CREATE INDEX [IX_BookingSeats_ShowSeatId] ON [dbo].[BookingSeats]([ShowSeatId]);
");

            // (G) Add FK
            migrationBuilder.Sql(@"
ALTER TABLE dbo.BookingSeats
ADD CONSTRAINT FK_BookingSeats_ShowSeats_ShowSeatId
    FOREIGN KEY (ShowSeatId) REFERENCES dbo.ShowSeats (ShowSeatId) ON DELETE NO ACTION;
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop FK
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

            // Drop INDEX
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

            // Relax column back to NULLABLE
            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.BookingSeats', 'ShowSeatId') IS NOT NULL
BEGIN
    ALTER TABLE dbo.BookingSeats ALTER COLUMN ShowSeatId bigint NULL;
END
");
            // We keep Shows.Date
        }
    }
}
