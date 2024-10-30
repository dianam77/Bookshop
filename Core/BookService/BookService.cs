using Core.FileUpload;
using DataAccess.Models;
using DataAccess.Repositories.BookRepo;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Core.BookService
{
    public class BookService
    {
        private readonly IBookRepository _bookRepository;
        private readonly IFileUploadService _fileUploadService;
        public BookService(IBookRepository bookRepository, IFileUploadService fileUploadService)
        {
            _bookRepository = bookRepository;
            _fileUploadService = fileUploadService;
        }


        public async Task<IEnumerable<Book>> GetBooks()
        {
            return await _bookRepository.GetAll().ToListAsync();
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
            var book = new Book()
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
                book.Img = await _fileUploadService.UploadFileAsync(bookdto.Img);
            }
            catch (Exception ex)
            {
               
                Console.WriteLine($"An error occurred while uploading the file: {ex.Message}");
            }

            await _bookRepository.Add(book);
        }
        public async Task UpdateBook(BookDto bookdto)
        {
            var book = await GetBookById(bookdto.Id);
            book.Title = bookdto.Title;
            book.Description = bookdto.Description;
            book.Price = bookdto.Price;
            book.AuthorId = bookdto.AuthorId;
            book.Created = DateTime.Now;
            book.IsAvail = bookdto.IsAvail;
            book.ShowHomePage = bookdto.ShowHomePage;
            if (bookdto.Img != null)
            {
                book.Img = await _fileUploadService.UploadFileAsync(bookdto.Img);
            }
            await _bookRepository.Update(book);
        }

        public async Task DeleteBook(Book book)
        {
            await _bookRepository.Delete(book);
        }

        public async Task<BookDto> GetBookDtoById(int id)
        {
            var book = await _bookRepository.GetById(id);

            if (book == null)
            {
                return null;  // Handle the case where the book is not found
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

            // Filter by search term if provided
            if (!string.IsNullOrEmpty(search))
            {
                books = books.Where(a => a.Title.Contains(search) || a.Description.Contains(search));
            }

            // Count total books for pagination
            int totalCount = await books.CountAsync();  // CountAsync for async operation
            int totalPage = (int)Math.Ceiling((double)totalCount / pageSize);

            // Apply pagination
            books = books.Skip((page - 1) * pageSize).Take(pageSize);

            // Include the Author entity, not the AuthorId
            books = books.Include(a => a.Author);

            // Select required fields for the DTO
            var bookDto = await books.Select(s => new BookDto()
            {
                Title = s.Title,
                Price = s.Price,
                AuthorId = s.AuthorId,  // AuthorId is just a scalar property, so you can include it directly here
                Description = s.Description,
                Id = s.Id,
                AuthorName = s.Author.Name,  // Access the related Author's Name
                ImgName = s.Img
            }).ToListAsync();

            // Prepare the final paginated result
            var result = new PagedBookDto()
            {
                Items = bookDto,
                page = page,
                totalPage = totalPage,
            };

            return result;
        }


    }
}
