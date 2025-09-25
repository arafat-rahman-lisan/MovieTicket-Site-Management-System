using System;
using System.Collections.Generic;
using Movie_Site_Management_System.Data.Enums;

namespace Movie_Site_Management_System.ViewModels.Payments
{
    public class PaymentListItemVM
    {
        public long PaymentId { get; set; }
        public string InvoiceNo { get; set; } = "";
        public long BookingId { get; set; }
        public string? CustomerEmail { get; set; }
        public string Method { get; set; } = "";
        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}