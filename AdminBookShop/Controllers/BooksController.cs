using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Core.BookService;
using Core.AuthorService;
using Core.ServiceFile;

namespace AdminBookShop.Controllers
{
    public class BooksController : Controller
    {
        private readonly BookService _bookService;
        private readonly AuthorService _authorService;
        private readonly IFileService _fileService;
        private IWebHostEnvironment _webHostEnvironment;

        public BooksController(BookService bookService, AuthorService authorService, IFileService fileService, IWebHostEnvironment webHostEnvironment)
        {
            _bookService = bookService;
            _authorService = authorService;
            _fileService = fileService; // Initialize the IFileService
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Books
        public async Task<IActionResult> Index()
        {
            var data = await _bookService.GetBooksWithAuthors();
            return View(data);
        }


        // GET: Books/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            //var books = await _bookService.GetBooksWithAuthors(a => a.Id == id);
            //var book = books.FirstOrDefault(); 
            var book = await _bookService.GetBookById(id.Value);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // GET: Books/Create
        public async Task<IActionResult> Create()
        {
            ViewData["AuthorId"] = new SelectList(await _authorService.GetAuthors(), "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookDto book, IFormFile Img)
        {
            // Check if the model is valid
            if (ModelState.IsValid)
            {
                
                // Proceed to create the book in the database
                await _bookService.CreateBook(book);

                // Redirect to the index action once the book is created
                return RedirectToAction(nameof(Index));
            }

            // If ModelState is invalid, reload the author dropdown list and return the view
            ViewData["AuthorId"] = new SelectList(await _authorService.GetAuthors(), "Id", "Name", book.AuthorId);
            return View(book);
        }


        // GET: Books/Edit/5
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

        // POST: Books/Edit/5
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
                catch (DbUpdateConcurrencyException ex)
                {
                    // Log and handle appropriately
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["AuthorId"] = new SelectList(await _authorService.GetAuthors(), "Id", "Name", book.AuthorId);
            return View(book);
        }

        // GET: Books/Delete/5
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
            var book = await _bookService.GetBookById(id); // Retrieve the book directly by ID.
            if (book == null)
            {
                return NotFound();
            }

            await _bookService.DeleteBook(book); // Perform the deletion.
            return RedirectToAction(nameof(Index)); // Redirect to the Index page after deletion.
        }

        [Route("/UploadFile")]
        [HttpPost]
        public IActionResult UploadFile(string path)
        {

            var files = Request.Form.Files;
            //var folderName = Path.Combine("Files", path);


            var folder = $@"Files\{path}\".Replace("\\", "/");
            var rootFolder = Path.Combine(_webHostEnvironment.WebRootPath, folder);

            if (!Directory.Exists(rootFolder))
            {
                Directory.CreateDirectory(rootFolder);
            }

            var file = files.FirstOrDefault();
            var newName = Guid.NewGuid().ToString();

            var fileInfo = new StaticFileUploadInfoDto();
            if (file != null && file.Length > 0)
            {
                string fileName = $"{newName}_{file.FileName}";
                var filePath = Path.Combine(rootFolder, fileName);



                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }

                fileInfo.FileAddress = $"/{folder}{fileName}";

            }


            return Ok(fileInfo);
        }

        [Route("/RemoveFile")]
        [HttpDelete]
        public async Task<IActionResult> DeleteFile(string path)
        {
            var rootPath = Path.Combine(_webHostEnvironment.WebRootPath, path.TrimStart('/'));
            try
            {
                // Attempt to force delete by changing file attributes
                System.IO.File.SetAttributes(rootPath, FileAttributes.Normal);
                System.IO.File.Delete(rootPath);
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Error deleting file: {ex.Message}");
            }

            return Ok();
        }

    }


}

