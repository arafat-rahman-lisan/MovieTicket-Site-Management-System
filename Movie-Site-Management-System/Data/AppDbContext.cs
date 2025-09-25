using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using Movie_Site_Management_System.Models;
using Movie_Site_Management_System.Data.Identity; // ✅ for ApplicationUser

namespace Movie_Site_Management_System.Data
{
    /// <summary>
    /// AppDbContext with ASP.NET Core Identity (ApplicationUser) + your domain entities.
    /// IMPORTANT: Inherits IdentityDbContext<ApplicationUser>.
    /// </summary>
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // DbSets (your domain tables)
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

        // ✅ New
        public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();
        public DbSet<Payment> Payments => Set<Payment>();

        protected override void OnModelCreating(ModelBuilder mb)
        {
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
              .OnDelete(DeleteBehavior.Restrict);

            mb.Entity<HallSlot>()
              .HasIndex(s => new { s.HallId, s.SlotNumber })
              .IsUnique();

            // TimeOnly conversions
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

            // ===== Movie: enums as strings
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

            // ===== ShowSeat (snapshot pricing & ShowSeatId PK)
            mb.Entity<ShowSeat>()
              .HasKey(ss => ss.ShowSeatId);

            mb.Entity<ShowSeat>()
              .HasIndex(ss => new { ss.ShowId, ss.SeatId })
              .IsUnique();

            mb.Entity<ShowSeat>()
              .HasOne(ss => ss.Show)
              .WithMany(s => s.ShowSeats)
              .HasForeignKey(ss => ss.ShowId)
              .OnDelete(DeleteBehavior.Cascade);

            mb.Entity<ShowSeat>()
              .HasOne(ss => ss.Seat)
              .WithMany(seat => seat.ShowSeats)
              .HasForeignKey(ss => ss.SeatId)
              .OnDelete(DeleteBehavior.Restrict);

            mb.Entity<ShowSeat>()
              .HasOne(ss => ss.SeatType)
              .WithMany()
              .HasForeignKey(ss => ss.SeatTypeId)
              .OnDelete(DeleteBehavior.Restrict);

            mb.Entity<ShowSeat>()
              .Property(p => p.Status)
              .HasConversion<string>()
              .HasMaxLength(10);

            mb.Entity<ShowSeat>()
              .Property(p => p.Price)
              .HasColumnType("decimal(10,2)");

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

            // ===== BookingSeat
            // Composite PK (BookingId, SeatId)
            mb.Entity<BookingSeat>()
              .HasKey(bs => new { bs.BookingId, bs.SeatId });

            // BookingSeat -> Booking (CASCADE)
            mb.Entity<BookingSeat>()
              .HasOne(bs => bs.Booking)
              .WithMany(b => b.BookingSeats)
              .HasForeignKey(bs => bs.BookingId)
              .OnDelete(DeleteBehavior.Cascade);

            // BookingSeat -> Seat (RESTRICT)
            mb.Entity<BookingSeat>()
              .HasOne(bs => bs.Seat)
              .WithMany(s => s.BookingSeats)
              .HasForeignKey(bs => bs.SeatId)
              .OnDelete(DeleteBehavior.Restrict);

            // ✅ BookingSeat -> ShowSeat (RESTRICT) via required FK ShowSeatId
            mb.Entity<BookingSeat>()
              .HasOne(bs => bs.ShowSeat)
              .WithMany(ss => ss.BookingSeats)
              .HasForeignKey(bs => bs.ShowSeatId)
              .OnDelete(DeleteBehavior.Restrict);

            // ===== SeatBlock
            mb.Entity<SeatBlock>()
              .HasOne(sb => sb.Seat)
              .WithMany(s => s.SeatBlocks)
              .HasForeignKey(sb => sb.SeatId)
              .OnDelete(DeleteBehavior.Cascade);

            // ===== ShowNote
            mb.Entity<ShowNote>()
              .HasOne(sn => sn.Show)
              .WithMany(s => s.ShowNotes)
              .HasForeignKey(sn => sn.ShowId)
              .OnDelete(DeleteBehavior.Cascade);

            // ===== PaymentMethod (lookup)
            mb.Entity<PaymentMethod>()
              .Property(pm => pm.Name)
              .HasMaxLength(50);

            mb.Entity<PaymentMethod>()
              .Property(pm => pm.CssClass)
              .HasMaxLength(20);

            mb.Entity<PaymentMethod>()
              .Property(pm => pm.LogoUrl)
              .HasMaxLength(200);

            // Seed methods (edit as you like)
            mb.Entity<PaymentMethod>().HasData(
                new PaymentMethod { PaymentMethodId = 1, Name = "PayPal", CssClass = "btn-dark" },
                new PaymentMethod { PaymentMethodId = 2, Name = "bKash", CssClass = "btn-pink" },
                new PaymentMethod { PaymentMethodId = 3, Name = "Nagad", CssClass = "btn-warning" }
            );

            // ===== Payment
            mb.Entity<Payment>()
              .HasIndex(p => p.InvoiceNo)
              .IsUnique();

            mb.Entity<Payment>()
              .Property(p => p.InvoiceNo)
              .HasMaxLength(32);

            mb.Entity<Payment>()
              .Property(p => p.ProviderTxnId)
              .HasMaxLength(64);

            mb.Entity<Payment>()
              .Property(p => p.ProviderRef)
              .HasMaxLength(64);

            mb.Entity<Payment>()
              .Property(p => p.Amount)
              .HasColumnType("decimal(10,2)");

            // Map PaymentStatus enum to string to match your style
            mb.Entity<Payment>()
              .Property(p => p.Status)
              .HasConversion<string>()
              .HasMaxLength(10);

            // Payment -> Booking (CASCADE)
            mb.Entity<Payment>()
              .HasOne(p => p.Booking)
              .WithMany(b => b.Payments) // if you added Booking.Payments
              .HasForeignKey(p => p.BookingId)
              .OnDelete(DeleteBehavior.Cascade);

            // Payment -> PaymentMethod (RESTRICT)
            mb.Entity<Payment>()
              .HasOne(p => p.PaymentMethod)
              .WithMany(pm => pm.Payments)
              .HasForeignKey(p => p.PaymentMethodId)
              .OnDelete(DeleteBehavior.Restrict);

            // IMPORTANT: call Identity base mapping LAST so Identity tables configure correctly.
            base.OnModelCreating(mb);
        }
    }

    // ---------- Converters & Comparers ----------
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
