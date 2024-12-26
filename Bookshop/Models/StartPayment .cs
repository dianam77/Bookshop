using Microsoft.AspNetCore.Mvc;

namespace Bookshop.Models
{
    public class StartPayment : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
