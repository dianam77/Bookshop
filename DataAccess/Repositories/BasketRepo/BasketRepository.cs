using DataAccess.Data;
using DataAccess.Models;
using DataAccess.Repositories.BasketRepo;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;


public class BasketRepository : IBasketRepository
{
    private readonly BookDbContext _context;
    public BasketRepository(BookDbContext context)
    {
        _context = context;
    }

    public IQueryable<Basket> GetAll(Expression<Func<Basket, bool>> where = null)
    {
        var baskets = _context.Baskets.AsQueryable();
        if (where != null)
        {
            baskets = baskets.Where(where);
        }
        return baskets;
    }

    public async Task Add(Basket basket)
    {
        _context.Baskets.Add(basket);
        await _context.SaveChangesAsync();
    }

    public async Task Delete(int id)
    {
        var basket = await GetById(id);
        _context.Baskets.Remove(basket);
        await _context.SaveChangesAsync();
    }

    public async Task Delete(Basket basket)
    {
        _context.Baskets.Remove(basket);
        await _context.SaveChangesAsync();
    }

    public async Task<Basket> GetById(int id)
    {
        return await _context.Baskets.FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task Update(Basket basket)
    {
        _context.Baskets.Update(basket);
        await _context.SaveChangesAsync();
    }

    public IQueryable<BasketItems> GetAllBasketItems(Expression<Func<BasketItems, bool>> where = null)
    {
        var basketItems = _context.BasketItems.AsQueryable();
        if (where != null)
        {
            basketItems = basketItems.Where(where);
        }
        return basketItems;
    }

    public async Task AddBasketItems(BasketItems items)
    {
        _context.BasketItems.Add(items);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteBasketItems(BasketItems items)
    {
        _context.BasketItems.Remove(items);
        await _context.SaveChangesAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task UpdateBasketItems(BasketItems item)
    {
        _context.BasketItems.Update(item);
        await _context.SaveChangesAsync();
    }
}
