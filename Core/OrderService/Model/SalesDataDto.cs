using System;

namespace Core.OrderService.Model
{
    // Change internal to public to make this class accessible outside the namespace
    public class SalesDataDto
    {
        // Properties representing the sales data
        public decimal TotalSales { get; set; }
        public int TotalOrders { get; set; }
        public int TotalCustomers { get; set; }
    }
}
