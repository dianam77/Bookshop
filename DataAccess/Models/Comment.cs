using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace DataAccess.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Text is required.")]
        [StringLength(1000, ErrorMessage = "Text cannot be longer than 1000 characters.")]
        public string Text { get; set; }

        [Required(ErrorMessage = "The Book field is required.")]
        public int BookId { get; set; }

        [ForeignKey("BookId")]
        public  Book? Book { get; set; }

        public string UserId { get; set; }

        public string UserName { get; set; }

        public DateTime Created { get; set; } = DateTime.Now;

        public int? ReplyId { get; set; }

        [ForeignKey("ReplyId")]
        public Comment? Reply { get; set; }

        // Removed the Required attribute from Replies
        public ICollection<Comment>? Replies { get; set; }
    }

}
