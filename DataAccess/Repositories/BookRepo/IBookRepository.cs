using DataAccess.Models;
using System.Linq.Expressions;

namespace DataAccess.Repositories.BookRepo
{
    public interface IBookRepository
    {
        IQueryable<Book> GetAll(Expression<Func<Book, bool>> where = null);
        Task<Book> GetById(int id);
        Task Add(Book book);
        Task Update(Book book);
        Task Delete(int id);
        Task Delete(Book book);

    }
}
