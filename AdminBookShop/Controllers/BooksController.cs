using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Core.BookService;
using Core.ServiceFile;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.Linq;  // Add this using directive

namespace AdminBookShop.Controllers
{
    public class BooksController : Controller
    {
        private readonly IBookService _bookService;
        private readonly IAuthorService _authorService;
        private readonly IFileService _fileService;
        private IWebHostEnvironment _webHostEnvironment;

        public BooksController(IBookService bookService, IAuthorService authorService, IFileService fileService, IWebHostEnvironment webHostEnvironment)
        {
            _bookService = bookService;
            _authorService = authorService;
            _fileService = fileService;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Books
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var data = await _bookService.GetBooksWithAuthors();
            return View(data);
        }

        // GET: Books/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _bookService.GetBookById(id.Value);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // GET: Books/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewData["AuthorId"] = new SelectList(await _authorService.GetAuthors(), "Id", "Name");
            return View();
        }

        // POST: Books/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookDto book, IFormFile Img)
        {
            if (ModelState.IsValid)
            {
                await _bookService.CreateBook(book);
                return RedirectToAction(nameof(Index));
            }

            ViewData["AuthorId"] = new SelectList(await _authorService.GetAuthors(), "Id", "Name", book.AuthorId);
            return View(book);
        }

        // GET: Books/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _bookService.GetBookDtoById(id.Value);
            if (book == null)
            {
                return NotFound();
            }

            ViewData["AuthorId"] = new SelectList(await _authorService.GetAuthors(), "Id", "Name", book.AuthorId);
            return View(book);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BookDto book)
        {
            if (id != book.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _bookService.UpdateBook(book);
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Log and handle the concurrency exception appropriately
                    // You can either reload the book or just return the view
                    ModelState.AddModelError("", "A concurrency error occurred. Please try again.");
                    ViewData["AuthorId"] = new SelectList(await _authorService.GetAuthors(), "Id", "Name", book.AuthorId);
                    return View(book);
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["AuthorId"] = new SelectList(await _authorService.GetAuthors(), "Id", "Name", book.AuthorId);
            return View(book);
        }

        

        // GET: Books/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _bookService.GetBookById(id.Value);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _bookService.GetBookById(id);
            if (book == null)
            {
                return NotFound();
            }

            await _bookService.DeleteBook(book);
            return RedirectToAction(nameof(Index));
        }

        [Route("/UploadFile")]
        [HttpPost]
        public IActionResult UploadFile(string path)
        {
            var files = Request.Form.Files;
            var folder = $@"Files\{path}\".Replace("\\", "/");
            var rootFolder = Path.Combine(_webHostEnvironment.WebRootPath, folder);

            if (!Directory.Exists(rootFolder))
            {
                Directory.CreateDirectory(rootFolder);
            }

            var file = files.FirstOrDefault();
            if (file == null || file.Length == 0)
            {
                return BadRequest("لطفاً یک فایل معتبر انتخاب کنید.");
            }

            // محدودیت حجم فایل (200KB)
            long maxFileSize = 200 * 1024;
            if (file.Length > maxFileSize)
            {
                return BadRequest("خطا: حجم فایل نباید بیشتر از 200 کیلوبایت باشد.");
            }

            // بررسی فرمت فایل (مجاز بودن تصاویر)
            var allowedExtensions = new List<string> { ".jpg", ".jpeg", ".png", ".gif" };
            var fileExtension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(fileExtension))
            {
                return BadRequest("فرمت فایل نامعتبر است. لطفاً یک تصویر با فرمت JPG، PNG یا GIF انتخاب کنید.");
            }

            string newName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(rootFolder, newName);

            try
            {
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving file: {ex.Message}");
                return StatusCode(500, "خطا در ذخیره‌سازی فایل. لطفاً مجدداً تلاش کنید.");
            }

            return Ok(new { FileAddress = $"/{folder}{newName}" });
        }



        // DELETE: Remove File
        [Route("/RemoveFile")]
        [HttpDelete]
        public async Task<IActionResult> DeleteFile(string path)
        {
            var rootPath = Path.Combine(_webHostEnvironment.WebRootPath, path.TrimStart('/'));
            try
            {
                await Task.Run(() =>
                {
                    System.IO.File.SetAttributes(rootPath, FileAttributes.Normal);
                    System.IO.File.Delete(rootPath);
                });
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Error deleting file: {ex.Message}");
            }

            return Ok();
        }
    }
}
