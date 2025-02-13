using AdminBookShop.Models;
using Core.OrderService;
using Core.OrderService.Model;
using Microsoft.AspNetCore.Mvc;

namespace AdminBookShop.Controllers
{
    public class HomeController : Controller
    {
        private readonly IOrderService _orderService;

        public HomeController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // GET: Orders (Index view)
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var orders = await _orderService.GetAllOrders();
            return View(orders); // Return the view with all orders
        }

        // GET: Display order for deletion
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var order = await _orderService.GetOrderWithId(id);
            if (order == null)
            {
                return NotFound(); // Return a NotFound result if the order doesn't exist
            }
            return View(order); // Return a view that confirms the deletion
        }

        // POST: Handle confirmed deletion
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _orderService.GetOrderWithId(id);
            if (order == null)
            {
                return NotFound(); // Return a NotFound result if the order doesn't exist
            }

            await _orderService.DeleteOrder(id); // Perform deletion
            return RedirectToAction(nameof(Index)); // Redirect to the Index page after deletion
        }

        // GET: Display order for editing
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var order = await _orderService.GetOrderWithId(id);
            if (order == null)
            {
                return NotFound(); // Return a NotFound result if the order doesn't exist
            }
            return View(order); // Return a view to edit the order
        }

        // POST: Handle the edit form submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AdmiOrderDto order)
        {
            if (id != order.Id)
            {
                return BadRequest(); // Return a BadRequest if the ID doesn't match
            }

            if (ModelState.IsValid)
            {
                // Update the order in the service
                await _orderService.UpdateOrder(order);
                return RedirectToAction(nameof(Index)); // Redirect to Index after update
            }

            return View(order); // Return the view with validation errors
        }
    }
}
