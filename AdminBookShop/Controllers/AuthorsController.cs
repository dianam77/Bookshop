using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DataAccess.Data;
using DataAccess.Models;
using Core.AuthorService;

namespace AdminBookShop.Controllers
{
    public class AuthorsController : Controller
    {
        private readonly AuthorService _authorService;


        public AuthorsController(AuthorService authorService)
        {
  
            _authorService = authorService;
        }

        // GET: Authors
        public async Task<IActionResult> Index()
        {
            return View(await _authorService.GetAuthors());
        }

        // get: authors/details/5
        public async Task<IActionResult> details(int? id)
        {
            if (id == null)
            {
                return  NotFound();
            }

            var author = await _authorService.GetAuthorById(id.Value);
            if (author == null)
            {
                return NotFound();
            }

            return View(author);
        }

        // GET: Authors/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Authors/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] Author author)
        {
            if (ModelState.IsValid)
            {
                await _authorService.CreateAuthor(author);
                return RedirectToAction(nameof(Index));
               
            }
            return View(author);

        }

        // GET: Authors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var author = await _authorService.GetAuthorById(id.Value);
            if (author == null)
            {
                return NotFound();
            }
            return View(author);
        }

        // POST: Authors/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Author author)
        {
            if (id != author.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _authorService.UpdateAuthor(author);

                    
                }
                catch (DbUpdateConcurrencyException)
                {

                }
                return RedirectToAction(nameof(Index));
            }
            return View(author);
        }


        // GET: Authors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var author = await _authorService.GetAuthorById(id.Value);
            if (author == null)
            {
                return NotFound();
            }

            return View(author);
        }

        // POST: Authors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var author = await _authorService.GetAuthorById(id);
            await _authorService.DeleteAuthor(author);
            return RedirectToAction(nameof(Index));
        }

    
    }
}
