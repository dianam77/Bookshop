using DataAccess.Enums;
using System.ComponentModel.DataAnnotations.Schema;


namespace DataAccess.Models
{
    public class BasketItems
    {
        [key]
        public int Id { get; set; }

        public int BasketId { get; set; }

        public int BookId { get; set; }

        public int Price { get; set; }

        public int Qty { get; set; }

        public DateTime Created { get; set; }

        [ForeignKey("BasketId")]
        public Basket Basket { get; set; }

        [ForeignKey("BookId")]
        public Book Book { get; set; }
        public Status Status { get; set; }
    }
}
