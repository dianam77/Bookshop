using Core.OrderService.Model;  // Ensure SalesDataDto's namespace is included
using System.Collections.Generic;
using static Core.OrderService.OrderService;

namespace AdminBookShop.Models
{
    public class OrdersIndexViewModel
    {
        public List<AdmiOrderDto> Orders { get; set; }
        public SalesDataDto SalesData { get; set; }
    }
}
