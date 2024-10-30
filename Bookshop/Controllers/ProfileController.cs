using Core.OrderService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Bookshop.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        
        private readonly OrderService _orderService;
        public ProfileController(OrderService orderService)
        {
            _orderService = orderService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // Fetch the user's paid orders
            var orders = await _orderService.GetUserOrders(userId);
            return View(orders);
        }
    }
}
