using DataAccess.Models;
using DataAccess.Repositories.AuthorRepo;

namespace Core.AuthorService
{
    public class AuthorService
    {
        private readonly IAuthorRepository _authorRepository;
        public AuthorService(IAuthorRepository authorRepository)
        {
            _authorRepository = authorRepository;
        }


        public async Task<IEnumerable<Author>> GetAuthors()
        {
            return await _authorRepository.GetAll();
        }

        public async Task<Author> GetAuthorById(int id)
        {
            return await _authorRepository.GetById(id);
        }

        public async Task CreateAuthor(Author author)
        {
            await _authorRepository.Add(author);
        }
        public async Task UpdateAuthor(Author author)
        {
            await _authorRepository.Update(author);
        }
        public async Task DeleteAuthor(Author author)
        {
            await _authorRepository.Delete(author);
        }
    }
}
