using DataAccess.Models;
using System.Linq.Expressions;


namespace DataAccess.Repositories.BasketRepo
{
    public interface IBasketRepository
    {
        IQueryable<Basket> GetAll(Expression<Func<Basket, bool>> where = null);
        Task<Basket> GetById(int id);
        Task Add(Basket basket);
        Task Update(Basket basket);
        Task Delete(int id);
        Task Delete(Basket basket);
        Task SaveChangesAsync();
        Task UpdateBasketItems(BasketItems item);
        IQueryable<BasketItems> GetAllBasketItems(Expression<Func<BasketItems, bool>> where = null);
        Task AddBasketItems(BasketItems items);
        Task DeleteBasketItems(BasketItems items);
    }
}