using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Core.BookService
{
    public class BookDto
    {
        [Key]
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public int Price { get; set; }

        public bool IsAvail { get; set; }

        public bool ShowHomePage { get; set; }
        public IFormFile? Img { get; set; }
        public string? ImgName { get; set; }

        public int AuthorId { get; set; }
        public string? AuthorName { get; set; }

        public decimal AverageRating { get; set; } // Added for rating
        public int RatingCount { get; set; } // Added for rating count
    }
}
