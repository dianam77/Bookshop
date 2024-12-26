using DataAccess.Models;

namespace DataAccess.Repositories.RateBookRepo
{
    public interface IRateBookRepository
    {
        Task Add(RateBookModel rateBook);
        Task<RateBookModel> GetRatingByUserAndBook(string userId, int bookId);
        Task Update(RateBookModel rateBook);  // Add Update method to the interface
    }
}

