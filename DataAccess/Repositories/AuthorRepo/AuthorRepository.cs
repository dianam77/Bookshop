﻿using DataAccess.Data;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories.AuthorRepo
{
    public class AuthorRepository : IAuthorRepository
    {
        private readonly BookDbContext _context;

        public AuthorRepository(BookDbContext context)
        {
            _context = context;
        }

        public async Task Add(Author author)
        {
            _context.Authors.Add(author);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var data = await GetById(id);
            _context.Authors.Remove(data);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(Author author)
        {
            _context.Authors.Remove(author);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Author>> GetAll()
        {
            var data = await _context.Authors.ToListAsync();
            return data;
        }

        public async Task<string> GetAuthorName(int id)
        {
            var author = await _context.Authors.FindAsync(id);
            return author?.Name ?? "Unknown Author";
        }

        public async Task<Author> GetById(int id)
        {
            var author = await _context.Authors.FirstOrDefaultAsync(a => a.Id == id);

            if (author == null)
            {
                throw new KeyNotFoundException($"Author with ID {id} not found.");
            }

            return author;
        }

        public async Task Update(Author author)
        {
            try
            {
                _context.Authors.Update(author);
                await _context.SaveChangesAsync();
            }
            catch
            {
                // Optionally handle the exception (e.g., display a message, etc.)
                throw; // Rethrow the exception
            }
        }

    }
}
