using System;
using System.Collections.Generic;
using Movie_Site_Management_System.Data.Enums;

namespace Movie_Site_Management_System.ViewModels.Payments
{
    public class PaymentsDashboardVM
    {
        // Filters & options
        public int Days { get; set; } = 30;
        public int? MethodId { get; set; }
        public PaymentStatus? Status { get; set; }
        public string? Q { get; set; }
        public List<(int Id, string Name)> Methods { get; set; } = new();

        // KPIs
        public int TotalCount { get; set; }
        public int PaidCount { get; set; }
        public int PendingCount { get; set; }
        public int FailedCount { get; set; }       // failed + cancelled
        public decimal PaidAmount { get; set; }    // revenue

        // Table
        public List<PaymentListItemVM> Items { get; set; } = new();

        // Optional sparkline data (amount/day)
        public List<(DateTime Day, decimal Amount)> RevenueSeries { get; set; } = new();
    }
}
