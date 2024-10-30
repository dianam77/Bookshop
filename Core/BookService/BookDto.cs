using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

  
    }
}
