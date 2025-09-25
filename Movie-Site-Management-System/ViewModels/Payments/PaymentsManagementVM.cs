using Movie_Site_Management_System.Data.Enums;
using Movie_Site_Management_System.Models;
using System.Collections.Generic;

namespace Movie_Site_Management_System.ViewModels.Payments
{
    public class PaymentsManagementVM
    {
        public int TotalPayments { get; set; }
        public decimal TotalAmount { get; set; }
        public Dictionary<PaymentStatus, (int Count, decimal Amount)> ByStatus { get; set; } = new();
        public List<PaymentMethod> Methods { get; set; } = new();
        public List<Payment> Latest { get; set; } = new();
    }
}
