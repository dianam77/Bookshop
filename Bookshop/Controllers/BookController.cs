using Bookshop.Models;
using Core.BookService;
using Core.ServiceFile;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using DataAccess.Repositories.RateBookRepo;
using Microsoft.AspNetCore.Http; 
using System.Web;             

namespace Bookshop.Controllers
{
    public class BookController : Controller
    {
        private readonly IBookService _bookService;
        private readonly IFileService _fileService;
        private readonly IRateBookRepository _rateBookRepository;

        public BookController(IBookService bookService, IFileService fileService, IRateBookRepository rateBookRepository)
        {
            _bookService = bookService;
            _fileService = fileService;
            _rateBookRepository = rateBookRepository;
        }


        [HttpGet]
        public async Task<IActionResult> Index(int id)
        {
            var book = await _bookService.GetBookById(id);  
            if (book == null)
            {
                return NotFound("Book not found.");
            }

            var booksBySameAuthor = await _bookService.GetBooksBySameAuthor(id, book.AuthorId); 
            var comments = await _bookService.GetCommentsByBookId(id);  

            var model = new BookWithSameAuthorViewModel
            {
                CurrentBook = book,
                BooksBySameAuthor = booksBySameAuthor,
                Comments = comments,
                CurrentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> RateBook([FromBody] RateBookModel model)
        {
            try
            {
                if (User.Identity?.IsAuthenticated != true)
                {
                    return Unauthorized("برای امتیاز دادن باید وارد سیستم شوید.");
                }

                // Validate that model.UserId is not null or whitespace
                if (string.IsNullOrWhiteSpace(model.UserId))
                {
                    return BadRequest("UserId is required to rate the book.");
                }

                var book = await _bookService.GetBookById(model.BookId);
                if (book == null)
                {
                    return BadRequest("کتاب یافت نشد.");
                }

                // Now that we've validated model.UserId, it's safe to use it.
                var existingRating = await _rateBookRepository.GetRatingByUserAndBook(model.UserId, model.BookId);

                if (existingRating != null)
                {
                    // Calculate new average rating
                    if (book.RatingCount > 1)
                    {
                        book.AverageRating = ((book.AverageRating * book.RatingCount) - existingRating.Rating + model.Rating) / book.RatingCount;
                    }
                    else
                    {
                        book.AverageRating = model.Rating;
                    }

                    // Update the existing rating
                    existingRating.Rating = model.Rating;
                    existingRating.RatedOn = DateTime.Now;

                    await _rateBookRepository.Update(existingRating);
                }
                else
                {
                    // Add new rating
                    book.RatingCount++;
                    if (book.RatingCount == 1)
                    {
                        book.AverageRating = model.Rating;
                    }
                    else
                    {
                        book.AverageRating = ((book.AverageRating * (book.RatingCount - 1)) + model.Rating) / book.RatingCount;
                    }

                    var newRating = new RateBookModel
                    {
                        UserId = model.UserId,
                        BookId = model.BookId,
                        Rating = model.Rating,
                        RatedOn = DateTime.Now
                    };

                    await _rateBookRepository.Add(newRating);
                }

                // Update the book information
                await _bookService.UpdateBook(MapToBookDto(book));

                // Return the result to the frontend
                return Json(new { success = true, averageRating = book.AverageRating, ratingCount = book.RatingCount });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"خطا: {ex.Message}" });
            }
        }

        private BookDto MapToBookDto(DataAccess.Models.Book book)
        {
            return new BookDto
            {
                Id = book.Id,
                Title = book.Title,
                Description = book.Description,
                Price = book.Price,
                IsAvail = book.IsAvail,
                ShowHomePage = book.ShowHomePage,
                Img = null, // Handle Img separately if needed
                AuthorId = book.AuthorId,
                AverageRating = book.AverageRating,
                RatingCount = book.RatingCount
            };
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddComment(int bookId, string text, int? replyId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var userName = User.Identity?.Name ?? string.Empty;

            var comment = new Comment
            {
                BookId = bookId,
                Text = text,
                Created = DateTime.Now,
                UserId = userId,
                UserName = userName,
                ReplyId = replyId
            };

            await _bookService.AddComment(comment);  
            return RedirectToAction("Index", new { id = bookId });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteComment(int id, int bookId)
        {
            var comment = await _bookService.GetCommentById(id); 
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (comment?.UserId?.Equals(userId) == true)
            {
                await _bookService.DeleteComment(id);
            }

            return RedirectToAction("Index", new { id = bookId });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> EditComment(int id, int bookId, string newText)
        {
            var comment = await _bookService.GetCommentById(id);  
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (comment!.UserId != null && comment.UserId == userId!)
            {
                comment.Text = newText;
                await _bookService.UpdateComment(comment);
            }

            return RedirectToAction("Index", new { id = bookId });
        }

        [HttpGet]
        public async Task<IActionResult> BookList(int page = 1, int pageSize = 6, string? search = null)
        {
            var data = await _bookService.GetBookPagination(page, pageSize, search);  
            return View(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetBook(int id)
        {
            var book = await _bookService.GetBookById(id);  
            return PartialView(book);
        }

        [HttpGet("/downloadFile/{*filePath}")]
        public async Task<IActionResult> DownloadFile(string filePath)
        {
            try
            {
                var fileName = Path.GetFileName(filePath);
                var memoryStream = await _fileService.DownloadFileAsync(filePath);  
                memoryStream.Position = 0;

                return File(memoryStream, "application/octet-stream", fileName);
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return NotFound("File not found in source project.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }
    }
}
