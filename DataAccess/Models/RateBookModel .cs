using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models
{
    public class RateBookModel
    {
        [Key]
        public int Id { get; set; }

        public int BookId { get; set; }

        [ForeignKey("BookId")]
        public Book? Book { get; set; }  // ✅ Nullable to prevent CS8603 warning

        public string? UserId { get; set; }
        public int Rating { get; set; }
        public DateTime RatedOn { get; set; } = DateTime.Now;
    }

}
