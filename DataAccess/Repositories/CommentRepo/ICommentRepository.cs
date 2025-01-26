using DataAccess.Models;


namespace DataAccess.Repositories.CommentRepo
{
    public interface ICommentRepository
    {
        Task Add(Comment comment); // Adds a new comment
        Task<IEnumerable<Comment>> GetCommentsByBookId(int bookId); // Retrieves comments by book ID
        Task<IEnumerable<Comment>> GetAllComments(); // Retrieves all comments
        Task<Comment> GetCommentById(int id); // Retrieves a single comment by its ID
        Task<IEnumerable<Comment>> GetRepliesByCommentId(int commentId); // Retrieves replies by comment ID
        Task Update(Comment comment); // Updates an existing comment
        Task Delete(int id); // Deletes a comment by its ID
    }
}
