using DataAccess.Models;
using System.Collections.Generic;

namespace Bookshop.Models
{
    public class BookWithSameAuthorViewModel
    {
        public Book CurrentBook { get; set; }
        public string CurrentUserId { get; set; }
        public IEnumerable<Comment> Comments { get; set; }
        public int? UserRating { get; set; }  // اضافه کردن امتیاز کاربر
        public IEnumerable<Book> BooksBySameAuthor { get; internal set; }
    }
}
