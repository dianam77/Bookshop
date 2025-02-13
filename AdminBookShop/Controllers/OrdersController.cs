using AdminBookShop.Models;
using Core.OrderService;
using Microsoft.AspNetCore.Mvc;


namespace AdminBookShop.Controllers
{
    public class OrdersController : Controller
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var data = await _orderService.GetAllOrders();
                return View(data);
            }
            catch (Exception)
            {
                // Log the exception here (optional)
                return View("Error");
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var data = await _orderService.GetOrderWithId(id);

                if (data == null)
                {
                    return NotFound(); // Return NotFound if the order doesn't exist
                }

                return View(data);
            }
            catch (Exception)
            {
                // Log the exception here (optional)
                return View("Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SetStatusCommand([FromBody] StatusDto model)
        {
            if (!ModelState.IsValid) // Check if the model is valid
            {
                return BadRequest(new { error = "Invalid model" }); // Return BadRequest if the model is invalid
            }

            try
            {
                var data = await _orderService.SetStatus(model.Id, model.State);
                return Ok(new { res = data });
            }
            catch (Exception)
            {
                // Log the exception here (optional)
                return BadRequest(new { error = "Service Error" });
            }
        }
    }
}