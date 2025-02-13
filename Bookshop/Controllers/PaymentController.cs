using Bookshop.Models;
using Core.OrderService;
using DataAccess.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Bookshop.Controllers
{
    public class PaymentController : Controller
    {
        // Inject IOrderService interface instead of concrete OrderService class
        private readonly IOrderService _orderService;

        public PaymentController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            // Logic for returning the payment form
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(decimal totalPrice)
        {
            // Fetch the basket items asynchronously (if necessary)
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var basketItems = await _orderService.GetUserBasket(userId);

            // Use the value from the form for totalAmount
            var totalAmount = totalPrice;

            // Create the Payment model with the total amount
            var paymentModel = new Payment
            {
                Amount = totalAmount // Set the total amount for the payment
            };

            return View(paymentModel); // Pass the Payment model to the view
        }

        [HttpPost]
        public async Task<IActionResult> ProcessPayment(Payment payment)
        {
            if (ModelState.IsValid)
            {
                // Simulate payment processing logic
                var paymentSuccess = await ProcessPaymentAsync(payment);

                if (paymentSuccess)
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

                    // Mark the items as paid and remove them from the basket
                    await _orderService.MarkBasketItemsAsPaid(userId);

                    // Redirect to the profile page to show the paid orders
                    return RedirectToAction("Index", "Profile");
                }
                else
                {
                    TempData["PaymentError"] = "Payment failed due to insufficient funds or invalid card details.";
                    return RedirectToAction("Failure");
                }
            }

            return View("Index", payment);
        }

        private async Task<bool> ProcessPaymentAsync(Payment payment)
        {
            // Simulate some payment processing logic
            try
            {
                // Here you would typically call an external payment service API
                await Task.Delay(1000); // Simulating network delay

                // For demonstration, let's say the payment succeeds
                return true; // Change this based on actual payment result
            }
            catch (Exception ex)
            {
                // Log the exception if necessary
                Console.WriteLine(ex.Message);
                return false; // Indicate that the payment failed
            }
        }

        [HttpGet]
        public IActionResult Failure()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Success()
        {
            return View();
        }
    }
}
