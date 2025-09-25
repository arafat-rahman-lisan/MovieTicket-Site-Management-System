using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Movie_Site_Management_System.Data;
using Movie_Site_Management_System.Models;
using Movie_Site_Management_System.Data.Enums;
using Movie_Site_Management_System.Services.Interfaces; // IEmailService
using Movie_Site_Management_System.ViewModels.Payments;
using Movie_Site_Management_System.Services.Invoices; // PDF service

namespace Movie_Site_Management_System.Controllers
{
    // [Authorize]
    public class PaymentsController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IInvoicePdfService _pdf;

        public PaymentsController(AppDbContext db, IInvoicePdfService pdf)
        {
            _db = db;
            _pdf = pdf;
        }

        // Admin: Payments Dashboard
        [HttpGet]
        // [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> Management(
            int days = 30,
            int? methodId = null,
            PaymentStatus? status = null,
            string? q = null)
        {
            var since = DateTime.UtcNow.AddDays(-Math.Max(1, days));

            var query = _db.Payments
                .AsNoTracking()
                .Include(p => p.PaymentMethod)
                .Include(p => p.Booking)!.ThenInclude(b => b.User)
                .Include(p => p.Booking)!.ThenInclude(b => b.Show)!.ThenInclude(s => s.Movie)
                .Where(p => p.CreatedAt >= since);

            if (methodId.HasValue)
                query = query.Where(p => p.PaymentMethodId == methodId.Value);

            if (status.HasValue)
                query = query.Where(p => p.Status == status.Value);

            if (!string.IsNullOrWhiteSpace(q))
            {
                query = query.Where(p =>
                    p.InvoiceNo.Contains(q) ||
                    (p.Booking!.User != null && p.Booking.User.Email!.Contains(q)) ||
                    (p.Booking!.Show!.Movie!.Title != null && p.Booking.Show.Movie.Title.Contains(q)));
            }

            var list = await query
                .OrderByDescending(p => p.PaymentId)
                .ToListAsync();

            var vm = new PaymentsDashboardVM
            {
                Days = days,
                MethodId = methodId,
                Status = status,
                Q = q ?? "",
                Methods = await _db.PaymentMethods
                    .Select(m => new ValueTuple<int, string>(m.PaymentMethodId, m.Name))
                    .ToListAsync(),

                TotalCount = list.Count,
                PaidCount = list.Count(p => p.Status == PaymentStatus.Paid),
                PendingCount = list.Count(p => p.Status == PaymentStatus.Pending),
                FailedCount = list.Count(p => p.Status == PaymentStatus.Failed || p.Status == PaymentStatus.Cancelled),
                PaidAmount = list.Where(p => p.Status == PaymentStatus.Paid).Sum(p => p.Amount),

                Items = list.Select(p => new PaymentListItemVM
                {
                    PaymentId = p.PaymentId,
                    InvoiceNo = p.InvoiceNo,
                    BookingId = p.BookingId,
                    CustomerEmail = p.Booking!.User?.Email ?? "(guest)",
                    Method = p.PaymentMethod!.Name,
                    Amount = p.Amount,
                    Status = p.Status,
                    CreatedAt = p.CreatedAt
                }).ToList()
            };

            // daily revenue series
            var startDate = DateTime.UtcNow.Date.AddDays(-Math.Max(1, days) + 1);
            var paid = list.Where(p => p.Status == PaymentStatus.Paid);

            vm.RevenueSeries = paid
                .GroupBy(p => p.CreatedAt.ToUniversalTime().Date)
                .Select(g => new ValueTuple<DateTime, decimal>(g.Key, g.Sum(x => x.Amount)))
                .ToList();

            for (var d = startDate; d <= DateTime.UtcNow.Date; d = d.AddDays(1))
                if (!vm.RevenueSeries.Any(x => x.Item1 == d))
                    vm.RevenueSeries.Add((d, 0m));

            vm.RevenueSeries = vm.RevenueSeries.OrderBy(x => x.Item1).ToList();

            return View("Management", vm);
        }

        // Invoice (choose method)
        [HttpGet]
        public async Task<IActionResult> Invoice(long bookingId)
        {
            var booking = await _db.Bookings
                .AsNoTracking()
                .Include(b => b.Show!).ThenInclude(s => s.Movie!)
                .Include(b => b.Show!).ThenInclude(s => s.HallSlot!)
                    .ThenInclude(hs => hs.Hall!).ThenInclude(h => h.Theatre!)
                .Include(b => b.BookingSeats!)
                    .ThenInclude(bs => bs.Seat!)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null)
                return NotFound();

            if (booking.Status == BookingStatus.CONFIRMED)
                return RedirectToAction("Details", "Bookings", new { id = bookingId });

            // methods for payment buttons
            ViewBag.PaymentMethods = await _db.PaymentMethods.AsNoTracking().ToListAsync();

            // expose flag in case you ever want to show the PDF after payment on this page
            ViewBag.CanDownload = await _db.Payments.AsNoTracking()
                .AnyAsync(p => p.BookingId == bookingId &&
                               (p.Status == PaymentStatus.Paid || p.Status == PaymentStatus.Success));

            return View("Invoice", booking);
        }

        // Download PDF Invoice - GATED: only after successful payment
        [HttpGet]
        public async Task<IActionResult> InvoicePdf(long bookingId)
        {
            try
            {
                var hasSuccessfulPayment = await _db.Payments
                    .AsNoTracking()
                    .AnyAsync(p => p.BookingId == bookingId &&
                                   (p.Status == PaymentStatus.Paid || p.Status == PaymentStatus.Success));

                if (!hasSuccessfulPayment)
                {
                    TempData["Error"] = "You can download the invoice only after successful payment.";
                    return RedirectToAction(nameof(Invoice), new { bookingId });
                }

                var pdfBytes = await _pdf.GenerateBookingInvoiceAsync(bookingId);
                var fileName = $"Invoice-INV-{bookingId:D6}.pdf";
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch
            {
                TempData["Error"] = "Could not generate invoice PDF.";
                return RedirectToAction(nameof(Invoice), new { bookingId });
            }
        }

        // Create Payment & go to mock gateway UI
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pay(long bookingId, int methodId)
        {
            var booking = await _db.Bookings
                .Include(b => b.BookingSeats!)
                    .ThenInclude(bs => bs.ShowSeat)
                .Include(b => b.Show)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null)
                return NotFound();

            if (booking.Status != BookingStatus.CREATED)
            {
                TempData["Error"] = "This booking is not in a payable state.";
                return RedirectToAction(nameof(Invoice), new { bookingId });
            }

            var method = await _db.PaymentMethods.FindAsync(methodId);
            if (method == null)
            {
                TempData["Error"] = "Invalid payment method.";
                return RedirectToAction(nameof(Invoice), new { bookingId });
            }

            var amount = booking.TotalAmount;
            if (amount <= 0m)
            {
                amount = booking.BookingSeats?
                    .Sum(bs => bs.ShowSeat != null ? bs.ShowSeat.Price : 0m) ?? 0m;
            }

            var payment = new Payment
            {
                BookingId = bookingId,
                PaymentMethodId = methodId,
                InvoiceNo = await GenerateInvoiceNoAsync(),
                Amount = amount,
                Status = PaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _db.Payments.Add(payment);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(GatewayDemo), new { paymentId = payment.PaymentId });
        }

        // Mock gateway screen
        [HttpGet]
        public async Task<IActionResult> GatewayDemo(long paymentId)
        {
            var payment = await _db.Payments
                .AsNoTracking()
                .Include(p => p.PaymentMethod)
                .Include(p => p.Booking!)
                    .ThenInclude(b => b.BookingSeats!)
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

            if (payment == null)
                return NotFound();

            if (payment.Status != PaymentStatus.Pending)
                return RedirectToAction(nameof(Invoice), new { bookingId = payment.BookingId });

            return View("GatewayDemo", payment);
        }

        // Confirm payment (mark paid, book seats)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmPayment(long paymentId)
        {
            var payment = await _db.Payments
                .Include(p => p.Booking!)
                    .ThenInclude(b => b.BookingSeats!)
                .Include(p => p.Booking!)
                    .ThenInclude(b => b.Show!)
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

            if (payment == null)
                return NotFound();

            var booking = payment.Booking!;
            if (payment.Status != PaymentStatus.Pending || booking.Status != BookingStatus.CREATED)
                return RedirectToAction(nameof(Invoice), new { bookingId = booking.BookingId });

            payment.Status = PaymentStatus.Paid;
            payment.PaidAt = DateTime.UtcNow;
            booking.Status = BookingStatus.CONFIRMED;

            var showSeatIds = booking.BookingSeats?
                .Select(bs => bs.ShowSeatId)
                .Where(id => id != 0)
                .Distinct()
                .ToList() ?? new List<long>();

            if (showSeatIds.Count > 0)
            {
                var showSeats = await _db.ShowSeats
                    .Where(ss => showSeatIds.Contains(ss.ShowSeatId))
                    .ToListAsync();

                foreach (var ss in showSeats)
                    ss.Status = ShowSeatStatus.Booked;
            }
            else
            {
                var seatIds = booking.BookingSeats?
                    .Select(bs => bs.SeatId)
                    .Where(id => id != 0)
                    .Distinct()
                    .ToList() ?? new List<long>();

                var showId = booking.Show?.ShowId ?? 0;
                if (showId != 0 && seatIds.Count > 0)
                {
                    var showSeats = await _db.ShowSeats
                        .Where(ss => ss.ShowId == showId && seatIds.Contains(ss.SeatId))
                        .ToListAsync();

                    foreach (var ss in showSeats)
                        ss.Status = ShowSeatStatus.Booked;
                }
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Success), new { paymentId = payment.PaymentId });
        }

        // Cancel a PENDING payment (unchanged)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelPayment(long paymentId)
        {
            var payment = await _db.Payments.FindAsync(paymentId);
            if (payment == null)
                return NotFound();

            if (payment.Status == PaymentStatus.Pending)
                payment.Status = PaymentStatus.Cancelled;

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Invoice), new { bookingId = payment.BookingId });
        }

        // >>> NEW: Admin cancel a PAID/SUCCESS payment (refund) and release seats
        [HttpPost]
        [ValidateAntiForgeryToken]
        // [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> AdminCancelPaid(long paymentId)
        {
            var payment = await _db.Payments
                .Include(p => p.Booking!)
                    .ThenInclude(b => b.BookingSeats!)
                .Include(p => p.Booking!)
                    .ThenInclude(b => b.Show)
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

            if (payment == null)
                return NotFound();

            if (payment.Status != PaymentStatus.Paid && payment.Status != PaymentStatus.Success)
            {
                TempData["Error"] = "Only paid payments can be cancelled here.";
                return RedirectToAction(nameof(Management));
            }

            var booking = payment.Booking!;
            // Release seats that were marked Booked
            var showSeatIds = booking.BookingSeats?
                .Select(bs => bs.ShowSeatId)
                .Where(id => id != 0)
                .Distinct()
                .ToList() ?? new List<long>();

            if (showSeatIds.Count > 0)
            {
                var showSeats = await _db.ShowSeats
                    .Where(ss => showSeatIds.Contains(ss.ShowSeatId))
                    .ToListAsync();

                foreach (var ss in showSeats)
                {
                    ss.Status = ShowSeatStatus.Available;
                    ss.HoldUntil = null;
                }
            }
            else
            {
                // Fallback by (ShowId, SeatId) if ShowSeatId snapshot wasn't stored
                var seatIds = booking.BookingSeats?
                    .Select(bs => bs.SeatId)
                    .Where(id => id != 0)
                    .Distinct()
                    .ToList() ?? new List<long>();

                var showId = booking.Show?.ShowId ?? 0;
                if (showId != 0 && seatIds.Count > 0)
                {
                    var showSeats = await _db.ShowSeats
                        .Where(ss => ss.ShowId == showId && seatIds.Contains(ss.SeatId))
                        .ToListAsync();

                    foreach (var ss in showSeats)
                    {
                        ss.Status = ShowSeatStatus.Available;
                        ss.HoldUntil = null;
                    }
                }
            }

            // Flip payment + booking
            payment.Status = PaymentStatus.Cancelled;
            // Optional: if you have a CancelledAt column, set it here
            // payment.CancelledAt = DateTime.UtcNow;

            // If your BookingStatus enum has CANCELLED, set it; otherwise leave CONFIRMED
            try
            {
                // This ‘as enum’ cast pattern avoids compile issues if CANCELLED doesn’t exist.
                var cancelledValue = Enum.Parse(typeof(BookingStatus), "CANCELLED", ignoreCase: true);
                booking.Status = (BookingStatus)cancelledValue;
            }
            catch
            {
                // no-op if CANCELLED doesn’t exist in your enum
            }

            await _db.SaveChangesAsync();
            TempData["Success"] = $"Payment #{payment.PaymentId} has been cancelled and seats were released.";
            return RedirectToAction(nameof(Management));
        }

        // Success page
        [HttpGet]
        public async Task<IActionResult> Success(long paymentId)
        {
            var payment = await _db.Payments
                .AsNoTracking()
                .Include(p => p.PaymentMethod)
                .Include(p => p.Booking!)
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

            if (payment == null)
                return NotFound();

            return View("Success", payment);
        }

        // Helpers
        private async Task<string> GenerateInvoiceNoAsync()
        {
            // Format: INV-YYYYMMDD-00001
            var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
            var prefix = $"INV-{datePart}-";

            var last = await _db.Payments
                .Where(p => p.InvoiceNo.StartsWith(prefix))
                .OrderByDescending(p => p.InvoiceNo)
                .Select(p => p.InvoiceNo)
                .FirstOrDefaultAsync();

            int seq = 1;
            if (!string.IsNullOrEmpty(last))
            {
                var pieces = last.Split('-');
                if (pieces.Length == 3 && int.TryParse(pieces[2], out var n))
                    seq = n + 1;
            }

            return $"{prefix}{seq:D5}";
        }
    }
}
