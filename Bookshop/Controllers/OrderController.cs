using Bookshop.Models;
using Core.OrderService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Bookshop.Controllers
{
    public class OrderController : Controller
    {
        private readonly OrderService _orderService;
        private readonly BasketRepository _basketRepository;

        public OrderController(OrderService orderService)
        {
            _orderService = orderService;
        }
        public IActionResult Index()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> AddToBasket([FromBody] AddBasketDto model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Ok(new { res = false, msg = "شما لاگین نکرده اید" });
            }

            var result = await _orderService.AddToBasket(model.bookId, model.qty, userId);
            return Ok(new { res = true });
        }


        [Authorize]
        public async Task<IActionResult> Basket()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var data = await _orderService.GetUserBasket(userId);
            return View(data);
        }

       
        [HttpPost]
        public async Task<IActionResult> RemoveBasket([FromBody] RemoveBasketDto model)
        {

            var res = await _orderService.RemoveItemBasket(model.Id);
            return Ok(new { res = true });
        }

    }
}
