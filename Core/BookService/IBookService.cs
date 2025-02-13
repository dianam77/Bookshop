using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Core.ServiceFile;
using DataAccess.Models;
using DataAccess.Repositories.BookRepo;
using DataAccess.Repositories.CommentRepo;
using DataAccess.Repositories.RateBookRepo;

namespace Core.BookService
{
    public interface IBookService
    {
        Task AddComment(Comment comment);
        Task<Comment?> GetCommentById(int id);
        Task<IEnumerable<Comment>> GetCommentsByBookId(int bookId);
        Task DeleteComment(int id);
        Task UpdateComment(Comment comment);
        Task<IEnumerable<Book>> GetBooks();
        Task<IEnumerable<Book>> GetBooksBySameAuthor(int bookId, int authorId);
        Task<IEnumerable<Book>> GetBooksWithAuthors(Expression<Func<Book, bool>> where = null!);
        Task<Book?> GetBookById(int id);
        Task CreateBook(BookDto bookdto);
        Task UpdateBook(BookDto bookdto);
        Task DeleteBook(Book book);
        Task<IEnumerable<Book>> GetBooksByAuthor(int authorId, int excludeBookId);
        Task<BookDto?> GetBookDtoById(int id);
        Task<PagedBookDto> GetBookPagination(int page, int pageSize, string? search);
        Task<RateBookModel> GetRatingByUserAndBook(string userId, int bookId);
        Task AddRating(RateBookModel rateBook);
    }
}
