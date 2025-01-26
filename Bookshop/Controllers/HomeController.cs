using Core.BookService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Core.ServiceFile;

namespace Bookshop.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly BookService _bookService;
        private readonly IFileService _fileService; // Add IFileService

        public HomeController(ILogger<HomeController> logger, BookService bookService, IFileService fileService)
        {
            _logger = logger;
            _bookService = bookService;
            _fileService = fileService; // Initialize the file service
        }

        [Authorize]
        public IActionResult AboutUs()
        {
            return View();
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }

    }
}