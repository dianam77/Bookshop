using Core.ServiceFile;
using DataAccess.Models;
using DataAccess.Repositories.BookRepo;
using DataAccess.Repositories.CommentRepo;
using DataAccess.Repositories.RateBookRepo; // Added using directive for RateBookRepo
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Core.BookService
{
    public class BookService
    {
        private readonly IBookRepository _bookRepository;
        private readonly IFileService _fileService;
        private readonly ICommentRepository _commentRepository;
        private readonly IRateBookRepository _rateBookRepository;

        public BookService(IBookRepository bookRepository, ICommentRepository commentRepository, IFileService fileService, IRateBookRepository rateBookRepository)
        {
            _bookRepository = bookRepository;
            _commentRepository = commentRepository;
            _fileService = fileService;
            _rateBookRepository = rateBookRepository;
        }

        public async Task AddComment(Comment comment)
        {
            await _commentRepository.Add(comment);
        }

        public async Task<Comment> GetCommentById(int id)
        {
            return await _commentRepository.GetCommentById(id);
        }

        public async Task<IEnumerable<Comment>> GetCommentsByBookId(int bookId)
        {
            return await _commentRepository.GetCommentsByBookId(bookId);
        }

        public async Task DeleteComment(int id)
        {
            await _commentRepository.Delete(id);
        }

        public async Task UpdateComment(Comment comment)
        {
            await _commentRepository.Update(comment);
        }

        public async Task<IEnumerable<Book>> GetBooks()
        {
            return await _bookRepository.GetAll().ToListAsync();
        }

        public async Task<IEnumerable<Book>> GetBooksBySameAuthor(int bookId, int authorId)
        {
            var booksBySameAuthor = await _bookRepository
                .GetAll()
                .Where(b => b.AuthorId == authorId && b.Id != bookId)
                .Include(b => b.Author)
                .ToListAsync();

            return booksBySameAuthor;
        }

        public async Task<IEnumerable<Book>> GetBooksWithAuthors(Expression<Func<Book, bool>> where = null)
        {
            return await _bookRepository.GetAll().Include(a => a.Author).ToListAsync();
        }

        public async Task<Book> GetBookById(int id)
        {
            return await _bookRepository.GetById(id);
        }

        public async Task CreateBook(BookDto bookdto)
        {
            var book = new Book
            {
                AuthorId = bookdto.AuthorId,
                Title = bookdto.Title,
                Description = bookdto.Description,
                Price = bookdto.Price,
                IsAvail = bookdto.IsAvail,
                ShowHomePage = bookdto.ShowHomePage,
                Created = DateTime.Now,
            };

            try
            {
                if (bookdto.Img != null)
                {
                    var result = await _fileService.UploadStaticFile(bookdto.Img, "BookImages");
                    book.Img = result.FileAddress;
                }

                await _bookRepository.Add(book);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while uploading the file or saving the book: {ex.Message}");
            }
        }

        public async Task UpdateBook(BookDto bookdto)
        {
            var book = await GetBookById(bookdto.Id);
            if (book == null)
            {
                throw new ArgumentException("Book not found");
            }

            book.Title = bookdto.Title;
            book.Description = bookdto.Description;
            book.Price = bookdto.Price;
            book.AuthorId = bookdto.AuthorId;
            book.IsAvail = bookdto.IsAvail;
            book.ShowHomePage = bookdto.ShowHomePage;
            book.AverageRating = bookdto.AverageRating;
            book.RatingCount = bookdto.RatingCount;

            if (bookdto.Img != null)
            {
                if (!string.IsNullOrEmpty(book.Img))
                {
                    _fileService.DeleteFile(book.Img);
                }

                var result = await _fileService.UploadStaticFile(bookdto.Img, "BookImages");
                book.Img = result.FileAddress;
            }

            await _bookRepository.Update(book);
        }

        public async Task DeleteBook(Book book)
        {
            await _bookRepository.Delete(book);
        }

        public async Task<IEnumerable<Book>> GetBooksByAuthor(int authorId, int excludeBookId)
        {
            return await _bookRepository.GetAll()
                .Where(b => b.AuthorId == authorId && b.Id != excludeBookId)
                .ToListAsync();
        }

        public async Task<BookDto> GetBookDtoById(int id)
        {
            var book = await _bookRepository.GetById(id);

            if (book == null)
            {
                return null;
            }

            var bookDto = new BookDto()
            {
                AuthorId = book.AuthorId,
                ImgName = book.Img,
                Description = book.Description,
                Id = id,
                Price = book.Price,
                Title = book.Title,
                IsAvail = book.IsAvail,
                ShowHomePage = book.ShowHomePage
            };

            return bookDto;
        }

        public async Task<PagedBookDto> GetBookPagination(int page, int pageSize, string? search)
        {
            var books = _bookRepository.GetAll();

            if (!string.IsNullOrEmpty(search))
            {
                books = books.Where(a => a.Title.Contains(search) || a.Description.Contains(search));
            }

            int totalCount = await books.CountAsync();
            int totalPage = (int)Math.Ceiling((double)totalCount / pageSize);

            books = books.Skip((page - 1) * pageSize).Take(pageSize);
            books = books.Include(a => a.Author);

            var bookDto = await books.Select(s => new BookDto()
            {
                Title = s.Title,
                Price = s.Price,
                AuthorId = s.AuthorId,
                Description = s.Description,
                Id = s.Id,
                AuthorName = s.Author.Name,
                ImgName = s.Img
            }).ToListAsync();

            var result = new PagedBookDto()
            {
                Items = bookDto,
                page = page,
                totalPage = totalPage,
            };

            return result;
        }

        // Added the new methods for rating
        public async Task<RateBookModel> GetRatingByUserAndBook(string userId, int bookId)
        {
            return await _rateBookRepository.GetRatingByUserAndBook(userId, bookId);
        }

        public async Task AddRating(RateBookModel rateBook)
        {
            await _rateBookRepository.Add(rateBook);
        }
    }
}