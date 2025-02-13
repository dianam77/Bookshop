using Core.OrderService.Model;  // Ensure SalesDataDto's namespace is included


namespace AdminBookShop.Models
{
    public class OrdersIndexViewModel
    {
        public List<AdmiOrderDto> Orders { get; set; } = new List<AdmiOrderDto>(); 
        public SalesDataDto SalesData { get; set; } = new SalesDataDto(); 
    }
}
