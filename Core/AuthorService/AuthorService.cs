using DataAccess.Models;
using DataAccess.Repositories.AuthorRepo;
using System.Collections;

namespace Core.AuthorService
{
    public class AuthorService : IAuthorService
    {
        private readonly IAuthorRepository _authorRepository;

        public AuthorService(IAuthorRepository authorRepository)
        {
            _authorRepository = authorRepository;
        }

        public async Task<IEnumerable<Author>> GetAllAuthorsAsync()
        {
            return await _authorRepository.GetAll();
        }

        public async Task<Author> GetAuthorByIdAsync(int id)
        {
            return await _authorRepository.GetById(id);
        }

        public async Task AddAuthorAsync(Author author)
        {
            await _authorRepository.Add(author);
        }

        public async Task UpdateAuthorAsync(Author author)
        {
            await _authorRepository.Update(author);
        }

        public async Task DeleteAuthorAsync(int id)
        {
            var author = await _authorRepository.GetById(id);
            if (author != null)
            {
                await _authorRepository.Delete(author);
            }
        }

        public async Task<IEnumerable> GetAuthors()
        {
            // Optionally, project the data to a lightweight format if needed.
            var authors = await _authorRepository.GetAll();
            return authors.Select(a => new { a.Id, a.Name });
        }
    }
}
