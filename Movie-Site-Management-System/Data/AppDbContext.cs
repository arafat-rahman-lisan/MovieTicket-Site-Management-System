using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Movie_Site_Management_System.Models;

namespace Movie_Site_Management_System.Data
{
    public class AppDbContext : DbContext
    {
       public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // DbSets
        public DbSet<Theatre> Theatres => Set<Theatre>();
        public DbSet<Hall> Halls => Set<Hall>();
        public DbSet<HallSlot> HallSlots => Set<HallSlot>();
        public DbSet<SeatType> SeatTypes => Set<SeatType>();
        public DbSet<Seat> Seats => Set<Seat>();
        public DbSet<Movie> Movies => Set<Movie>();
        public DbSet<Show> Shows => Set<Show>();
        public DbSet<ShowSeat> ShowSeats => Set<ShowSeat>();
        public DbSet<Booking> Bookings => Set<Booking>();
        public DbSet<BookingSeat> BookingSeats => Set<BookingSeat>();
        public DbSet<SeatBlock> SeatBlocks => Set<SeatBlock>();
        public DbSet<ShowNote> ShowNotes => Set<ShowNote>();

        protected override void OnModelCreating(ModelBuilder mb)
        {
            base.OnModelCreating(mb);

            // ===== Theatre =====
            mb.Entity<Theatre>()
              .Property(p => p.Lat).HasColumnType("decimal(9,6)");
            mb.Entity<Theatre>()
              .Property(p => p.Lng).HasColumnType("decimal(9,6)");

            // ===== Hall: (N)->(1) Theatre, unique (TheatreId, Name)
            mb.Entity<Hall>()
              .HasOne(h => h.Theatre)
              .WithMany(t => t.Halls)
              .HasForeignKey(h => h.TheatreId)
              .OnDelete(DeleteBehavior.Restrict);

            mb.Entity<Hall>()
              .HasIndex(h => new { h.TheatreId, h.Name })
              .IsUnique();

            // ===== HallSlot: (N)->(1) Hall, unique (HallId, SlotNumber)
            mb.Entity<HallSlot>()
              .HasOne(s => s.Hall)
              .WithMany(h => h.HallSlots)
              .HasForeignKey(s => s.HallId)
              .OnDelete(DeleteBehavior.Cascade);

            mb.Entity<HallSlot>()
              .HasIndex(s => new { s.HallId, s.SlotNumber })
              .IsUnique();

            // TimeOnly conversions (EF Core 7/8)
            mb.Entity<HallSlot>()
              .Property(s => s.StartTime)
              .HasConversion<TimeOnlyToTimeSpanConverter, TimeOnlyComparer>();
            mb.Entity<HallSlot>()
              .Property(s => s.EndTime)
              .HasConversion<TimeOnlyToTimeSpanConverter, TimeOnlyComparer>();

            // ===== Seat: (N)->(1) Hall (cascade), (N)->(1) SeatType (restrict)
            mb.Entity<Seat>()
              .HasOne(s => s.Hall)
              .WithMany(h => h.Seats)
              .HasForeignKey(s => s.HallId)
              .OnDelete(DeleteBehavior.Cascade);

            mb.Entity<Seat>()
              .HasOne(s => s.SeatType)
              .WithMany(st => st.Seats)
              .HasForeignKey(s => s.SeatTypeId)
              .OnDelete(DeleteBehavior.Restrict);

            mb.Entity<Seat>()
              .HasIndex(s => new { s.HallId, s.RowLabel, s.SeatNumber })
              .IsUnique();

            // ===== Movie: enums as strings (migration friendly)
            mb.Entity<Movie>()
              .Property(m => m.Status)
              .HasConversion<string>()
              .HasMaxLength(20);

            mb.Entity<Movie>()
              .Property(m => m.Genre)
              .HasConversion<string>()
              .HasMaxLength(20);

            // ===== Show: (N)->(1) Movie (restrict), (N)->(1) HallSlot (restrict)
            mb.Entity<Show>()
              .HasOne(s => s.Movie)
              .WithMany(m => m.Shows)
              .HasForeignKey(s => s.MovieId)
              .OnDelete(DeleteBehavior.Restrict);

            mb.Entity<Show>()
              .HasOne(s => s.HallSlot)
              .WithMany(sl => sl.Shows)
              .HasForeignKey(s => s.HallSlotId)
              .OnDelete(DeleteBehavior.Restrict);

            // Unique one show per hall-slot per date
            mb.Entity<Show>()
              .HasIndex(s => new { s.HallSlotId, s.ShowDate })
              .IsUnique();

            // DateOnly conversion
            mb.Entity<Show>()
              .Property(p => p.ShowDate)
              .HasConversion<DateOnlyToDateTimeConverter, DateOnlyComparer>();

            // ===== ShowSeat: composite PK, (N)->(1) Show, (N)->(1) Seat
            mb.Entity<ShowSeat>()
              .HasKey(ss => new { ss.ShowId, ss.SeatId });

            mb.Entity<ShowSeat>()
              .HasOne(ss => ss.Show)
              .WithMany(s => s.ShowSeats)
              .HasForeignKey(ss => ss.ShowId)
              .OnDelete(DeleteBehavior.Cascade);

            mb.Entity<ShowSeat>()
              .HasOne(ss => ss.Seat)
              .WithMany(seat => seat.ShowSeats)
              .HasForeignKey(ss => ss.SeatId)
              .OnDelete(DeleteBehavior.Cascade);

            mb.Entity<ShowSeat>()
              .Property(p => p.Status)
              .HasConversion<string>()
              .HasMaxLength(10);

            // ===== Booking: (N)->(1) Show (restrict)
            mb.Entity<Booking>()
              .HasOne(b => b.Show)
              .WithMany(s => s.Bookings)
              .HasForeignKey(b => b.ShowId)
              .OnDelete(DeleteBehavior.Restrict);

            mb.Entity<Booking>()
              .Property(p => p.Status)
              .HasConversion<string>()
              .HasMaxLength(10);

            // ===== BookingSeat: composite PK, to Booking (cascade) + Seat (restrict)
            mb.Entity<BookingSeat>()
              .HasKey(bs => new { bs.BookingId, bs.SeatId });

            mb.Entity<BookingSeat>()
              .HasOne(bs => bs.Booking)
              .WithMany(b => b.BookingSeats)
              .HasForeignKey(bs => bs.BookingId)
              .OnDelete(DeleteBehavior.Cascade);

            mb.Entity<BookingSeat>()
              .HasOne(bs => bs.Seat)
              .WithMany(s => s.BookingSeats)
              .HasForeignKey(bs => bs.SeatId)
              .OnDelete(DeleteBehavior.Restrict);

            // ===== SeatBlock: (N)->(1) Seat (cascade)
            mb.Entity<SeatBlock>()
              .HasOne(sb => sb.Seat)
              .WithMany(s => s.SeatBlocks)
              .HasForeignKey(sb => sb.SeatId)
              .OnDelete(DeleteBehavior.Cascade);

            // ===== ShowNote: (N)->(1) Show (cascade)
            mb.Entity<ShowNote>()
              .HasOne(sn => sn.Show)
              .WithMany(s => s.ShowNotes)
              .HasForeignKey(sn => sn.ShowId)
              .OnDelete(DeleteBehavior.Cascade);
        }
    }

    // ---------- Converters & Comparers ----------
    // DateOnly <-> DateTime
    public sealed class DateOnlyToDateTimeConverter : ValueConverter<DateOnly, DateTime>
    {
        public DateOnlyToDateTimeConverter() :
            base(d => d.ToDateTime(TimeOnly.MinValue),
                 d => DateOnly.FromDateTime(d))
        { }
    }

    public sealed class DateOnlyComparer : ValueComparer<DateOnly>
    {
        public DateOnlyComparer() :
            base((a, b) => a.DayNumber == b.DayNumber,
                 d => d.GetHashCode())
        { }
    }

    // TimeOnly <-> TimeSpan
    public sealed class TimeOnlyToTimeSpanConverter : ValueConverter<TimeOnly, TimeSpan>
    {
        public TimeOnlyToTimeSpanConverter() :
            base(t => t.ToTimeSpan(),
                 t => TimeOnly.FromTimeSpan(t))
        { }
    }

    public sealed class TimeOnlyComparer : ValueComparer<TimeOnly>
    {
        public TimeOnlyComparer() :
            base((a, b) => a.Ticks == b.Ticks,
                 t => t.GetHashCode())
        { }
    }

}
