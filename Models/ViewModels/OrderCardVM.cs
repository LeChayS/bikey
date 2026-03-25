using System;

namespace bikey.Models.ViewModels
{
    public class OrderCardVM
    {
        public string OrderCode { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public string StatusText { get; set; } = string.Empty;

        public string CustomerName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public string VehicleName { get; set; } = string.Empty;
        public string Plate { get; set; } = string.Empty;
        public decimal PricePerDay { get; set; }

        public int TotalDays { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public decimal TotalPrice { get; set; }

        public string? Note { get; set; }

        public int Id { get; set; }
    }
}