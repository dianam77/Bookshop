using DataAccess.Models;
using System.Collections.Generic;

namespace Bookshop.Models
{
    public class BookWithSameAuthorViewModel
    {
        public Book CurrentBook { get; set; } = new Book();  // Initialize with a new Book object
        public string CurrentUserId { get; set; } = string.Empty;  // Initialize with an empty string
        public IEnumerable<Comment> Comments { get; set; } = new List<Comment>();  // Initialize with an empty list
        public int? UserRating { get; set; }  // Nullable, no initialization needed
        public IEnumerable<Book> BooksBySameAuthor { get; set; } = new List<Book>();  // Initialize with an empty list
    }

}
