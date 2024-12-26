using System;
using System.Collections.Generic;
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
        public Book Book { get; set; }
        public string UserId { get; set; } // ذخیره ID کاربر
        public int Rating { get; set; }
        public DateTime RatedOn { get; set; } = DateTime.Now;
    }

}
