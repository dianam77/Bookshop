using Core.BookService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Core.ServiceFile;
using Microsoft.Extensions.Logging;

namespace Bookshop.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IBookService _bookService;
        private readonly IFileService _fileService;

        public HomeController(ILogger<HomeController> logger, IBookService bookService, IFileService fileService)
        {
            _logger = logger;
            _bookService = bookService;
            _fileService = fileService;
        }

        [HttpGet]
        [Authorize]
        public IActionResult AboutUs()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}
