using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.BookService
{
    public class PagedBookDto
    {
        public int totalPage { get; set; }

        public int page { get; set; }

        public List<BookDto> Items { get; set; }
    }
}
