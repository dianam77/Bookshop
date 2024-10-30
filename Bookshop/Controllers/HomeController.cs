using Bookshop.Models;
using Core.BookService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Bookshop.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly BookService _bookService;

        public HomeController(ILogger<HomeController> logger, BookService bookService = null)
        {
            _logger = logger;
            _bookService = bookService;
        }


        [Authorize]
        public IActionResult aboutUs()
        {
            return View();
        }
        public async Task<IActionResult> index()
        {
            
            return View();
        }

     
      
    }
}
