using DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using DataAccess.Data;

namespace DataAccess.Repositories.CommentRepo
{
    public class CommentRepository : ICommentRepository
    {
        private readonly BookDbContext _context;

        public CommentRepository(BookDbContext context)
        {
            _context = context;
        }

        public async Task Add(Comment comment)
        {
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Comment>> GetCommentsByBookId(int bookId)
        {
            return await _context.Comments.Where(c => c.BookId == bookId).ToListAsync();
        }

        public async Task<IEnumerable<Comment>> GetAllComments()
        {
            return await _context.Comments.ToListAsync();
        }

        public async Task<Comment> GetCommentById(int id)
        {
            return await _context.Comments.Include(c => c.Replies).FirstOrDefaultAsync(c => c.Id == id);
        }


        public async Task<IEnumerable<Comment>> GetRepliesByCommentId(int commentId)
        {
            return await _context.Comments.Where(c => c.ReplyId == commentId).ToListAsync();
        }

        public async Task Update(Comment reply)
        {
            _context.Comments.Update(reply);
            await _context.SaveChangesAsync();
        }
        public async Task Delete(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment != null)
            {
                _context.Comments.Remove(comment);
                await _context.SaveChangesAsync();
            }
        }
    }
}
