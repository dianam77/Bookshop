using Bookshop.Models;
using Core.OrderService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Bookshop.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Basket()
        {
            // Get the user ID from the claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // If user is not authenticated, redirect to the login page
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Get the user's basket data
            var data = await _orderService.GetUserBasket(userId);

            // Return the view with the basket data
            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> AddToBasket([FromBody] AddBasketDto model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) // Check if the user is authenticated
            {
                return Ok(new { res = false, msg = "شما لاگین نکرده اید" });
            }

            var result = await _orderService.AddToBasket(model.bookId, model.qty, userId);
            return Ok(new { res = result });
        }


        [HttpPost]
        public async Task<IActionResult> RemoveBasket([FromBody] RemoveBasketDto model)
        {
            var result = await _orderService.RemoveItemBasket(model.Id);
            return Ok(new { res = result });
        }
    }
}
