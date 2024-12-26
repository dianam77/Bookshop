using DataAccess.Data;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;


namespace DataAccess.Repositories.RateBookRepo
{
    public class RateBookRepository : IRateBookRepository
    {
        private readonly BookDbContext _context;

        public RateBookRepository(BookDbContext context)
        {
            _context = context;
        }

        public async Task Add(RateBookModel rateBook)
        {
            await _context.RateBooks.AddAsync(rateBook);
            await _context.SaveChangesAsync();
        }

        public async Task<RateBookModel> GetRatingByUserAndBook(string userId, int bookId)
        {
            return await _context.RateBooks
                .FirstOrDefaultAsync(r => r.UserId == userId && r.BookId == bookId);
        }

        public async Task Update(RateBookModel rateBook)
        {
            var existingRating = await _context.RateBooks
                .FirstOrDefaultAsync(r => r.UserId == rateBook.UserId && r.BookId == rateBook.BookId);

            if (existingRating != null)
            {
                existingRating.Rating = rateBook.Rating;
                existingRating.RatedOn = DateTime.Now;

                await _context.SaveChangesAsync();
            }
        }

     

    }
}
