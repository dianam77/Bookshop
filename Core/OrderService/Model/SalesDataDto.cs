
namespace Core.OrderService.Model
{
    // The class is already public, making it accessible outside the namespace
    public class SalesDataDto
    {
        // Properties representing the sales data, all of which are public
        public decimal TotalSales { get; set; }
        public int TotalOrders { get; set; }
        public int TotalCustomers { get; set; }
    }
}
