using AdminBookShop.Models;
using Core.OrderService;
using Microsoft.AspNetCore.Mvc;

namespace AdminBookShop.Controllers
{
    public class HomeController : Controller
    {
        private readonly IOrderService _orderService; // Inject the IOrderService interface

        public HomeController(IOrderService orderService) // Constructor now accepts IOrderService
        {
            _orderService = orderService;
        }

        public async Task<IActionResult> Index()
        {
            var data = await _orderService.GetAllOrders(); // Using IOrderService to fetch all orders
            return View(data);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var data = await _orderService.GetOrderWithId(id); // Using IOrderService to fetch a specific order by ID
            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> SetStatusCommand([FromBody] StatusDto model)
        {
            var data = await _orderService.SetStatus(model.Id, model.State); // Setting the status via IOrderService
            return Ok(new { res = data });
        }
    }
}
