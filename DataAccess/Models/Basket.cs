using DataAccess.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace DataAccess.Models
{
    public class Basket
    {
        [Key]
        public int Id { get; set; }

        public DateTime Created { get; set; }
        public DateTime Payed { get; set; }

        public string UserId { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public Status Status { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

        public ICollection<BasketItems>? BasketItems { get; set; }

    }
}
