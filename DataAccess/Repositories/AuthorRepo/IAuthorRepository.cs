using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories.AuthorRepo
{
    public interface IAuthorRepository
    {
        Task<IEnumerable<Author>> GetAll();

        Task<Author> GetById(int id);

        Task Add(Author author);
        Task Update(Author author);
        Task Delete(int id);
        Task Delete(Author author);
    }
}
