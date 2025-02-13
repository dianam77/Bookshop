﻿using Core.BookService;
using Microsoft.AspNetCore.Mvc;

namespace Bookshop.Component
{
    public class HomeBook : ViewComponent
    {
        private readonly IBookService _bookService;
        public HomeBook(IBookService bookService)
        {
            _bookService = bookService;
        }
        public async Task<IViewComponentResult> InvokeAsync(bool ShowHome, bool IsNew)
        {
            if (IsNew)
            {
                var data = await _bookService.GetBooksWithAuthors(a => a.ShowHomePage == ShowHome);
                data = data.OrderByDescending(a => a.Created).Take(6).ToList();
                return View("/Views/Shared/Component/HomeBook.cshtml", data);
            }
            else
            {
                var data = await _bookService.GetBooksWithAuthors(a => a.ShowHomePage == ShowHome);
                return View("/Views/Shared/Component/HomeBook.cshtml", data);
            }
        }
    }
}
