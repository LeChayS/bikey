using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bikey.Models.ViewModels
{
    public class OrderCardVM
    {
        public string OrderCode { get; set; }
        public DateTime OrderDate { get; set; }
        public string StatusText { get; set; }

        public string CustomerName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }

        public string VehicleName { get; set; }
        public string Plate { get; set; }
        public decimal PricePerDay { get; set; }

        public int TotalDays { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public decimal TotalPrice { get; set; }

        public string? Note { get; set; }

        public int Id { get; set; }
    }
}