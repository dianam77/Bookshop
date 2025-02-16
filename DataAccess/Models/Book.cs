using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models
{
    public class Book
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Price { get; set; }
        public string? Img { get; set; }
        public DateTime Created { get; set; }
        public bool IsAvail { get; set; }
        public bool ShowHomePage { get; set; }
        public int AuthorId { get; set; }
        [ForeignKey("AuthorId")]
        public Author? Author { get; set; }
        public ICollection<BasketItems>? BasketItems { get; set; }
        public ICollection<Comment>? Comment { get; set; }
        public ICollection<RateBookModel>? Ratings { get; set; } 
        public decimal AverageRating { get; set; } 
        public int RatingCount { get; set; }
      
    }
}

