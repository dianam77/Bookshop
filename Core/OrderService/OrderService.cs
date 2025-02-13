using Core.OrderService.Model;
using DataAccess.Models;
using DataAccess.Repositories.BasketRepo;
using DataAccess.Repositories.BookRepo;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.OrderService
{
    public class OrderService : IOrderService
    {
        private readonly IBasketRepository _basketRepository;
        private readonly IBookRepository _bookRepository;

        // Constructor assuming DI guarantees non-null values
        public OrderService(IBasketRepository basketRepository, IBookRepository bookRepository)
        {
            _basketRepository = basketRepository ?? throw new ArgumentNullException(nameof(basketRepository)); // Added argument validation
            _bookRepository = bookRepository ?? throw new ArgumentNullException(nameof(bookRepository));
        }

        public async Task<bool> AddToBasket(int bookId, int qty, string userId)
        {
            var basket = await _basketRepository.GetAll(a => a.UserId == userId && a.Status == DataAccess.Enums.Status.Created).FirstOrDefaultAsync();

            if (basket == null)
            {
                basket = new Basket
                {
                    UserId = userId,
                    Created = DateTime.Now,
                    Status = DataAccess.Enums.Status.Created,
                    Address = string.Empty,
                    Mobile = string.Empty,
                    Payed = DateTime.Now,
                };
                await _basketRepository.Add(basket);
            }

            var book = await _bookRepository.GetById(bookId);
            if (book == null) return false;

            var basketItem = new BasketItems
            {
                BasketId = basket.Id,
                Qty = qty,
                BookId = book.Id,
                Created = DateTime.Now,
                Price = book.Price * qty
            };
            await _basketRepository.AddBasketItems(basketItem);

            return true;
        }
        public async Task<List<BasketItems?>> GetUserBasket(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                // Return an empty list if userId is null or empty.
                return new List<BasketItems?>();
            }

            try
            {
                // Get baskets (allow nullable types as baskets can be null)
                var baskets = await _basketRepository
                    .GetAllBasketItems(a => a.Basket != null
                                            && a.Basket.UserId == userId
                                            && a.Basket.Status == DataAccess.Enums.Status.Created)
                    .Include(a => a.Basket)
                    .Include(a => a.Book)
                    .AsNoTracking()
                    .ToListAsync();

                // Convert baskets to List<BasketItems?>.
                return (baskets ?? new List<BasketItems>())
                            .Cast<BasketItems?>()
                            .ToList();
            }
            catch (Exception ex)
            {
                // Log the exception (consider using a proper logging framework)
                Console.WriteLine($"An error occurred: {ex.Message}");

                // Return an empty list on exception.
                return new List<BasketItems?>();
            }
        }


        public async Task<bool> RemoveItemBasket(int id)
        {
            var basketItem = await _basketRepository.GetAllBasketItems(a => a.Id == id).FirstOrDefaultAsync();
            if (basketItem != null)
            {
                await _basketRepository.DeleteBasketItems(basketItem);
                return true;
            }
            return false;
        }

        public async Task<bool> Pay(string address, string mobile, string userId)
        {
            var basket = await _basketRepository.GetAll(a => a.UserId == userId && a.Status == DataAccess.Enums.Status.Created).FirstOrDefaultAsync();
            if (basket == null) return false;

            basket.Mobile = mobile;
            basket.Address = address;
            basket.Payed = DateTime.Now;
            basket.Status = DataAccess.Enums.Status.Final;

            await _basketRepository.Update(basket);
            return true;
        }

        public async Task<List<Basket?>> GetUserOrders(string userId)
        {
            var baskets = await _basketRepository
                .GetAll(a => a.UserId == userId && a.Status != DataAccess.Enums.Status.Created)
                .Include(a => a.BasketItems!)
                .ThenInclude(a => a.Book)
                .AsNoTracking()
                .ToListAsync();
            return baskets!;
        }

        public async Task<List<AdmiOrderDto>> GetAllOrders()
        {
            return await _basketRepository
                .GetAll(a => a.Status != DataAccess.Enums.Status.Created)
                .Include(a => a.User!)
                .Include(a => a.BasketItems!)
                .ThenInclude(a => a.Book!)
                .Select(s => new AdmiOrderDto
                {
                    Address = s.Address,
                    Id = s.Id,
                    Mobile = s.Mobile,
                    Status = s.Status,
                    Payed = s.Payed,
                    UserId = s.UserId,
                    UserName = s.User!.UserName ?? string.Empty,
                    Items = s.BasketItems!.Select(c => c.Book!.Title).ToList()
                })
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<AdmiOrderDto?> GetOrderWithId(int id)
        {
            return await _basketRepository
                .GetAll(a => a.Id == id)
                .Include(a => a.User!)
                .Include(a => a.BasketItems!)
                .ThenInclude(a => a.Book!)
                .Select(s => new AdmiOrderDto
                {
                    Address = s.Address,
                    Id = s.Id,
                    Mobile = s.Mobile,
                    Status = s.Status,
                    Payed = s.Payed,
                    UserId = s.UserId,
                    UserName = s.User!.UserName ?? string.Empty,
                    Items = s.BasketItems!.Select(c => c.Book!.Title).ToList()
                })
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<bool> SetStatus(int id, bool state)
        {
            var basket = await _basketRepository.GetAll(a => a.Id == id).FirstOrDefaultAsync();
            if (basket != null)
            {
                basket.Status = state ? DataAccess.Enums.Status.Accepted : DataAccess.Enums.Status.Rejected;
                await _basketRepository.Update(basket);
                return true;
            }
            return false;
        }

        public async Task<bool> MarkBasketItemsAsPaid(string userId)
        {
            try
            {
                var basket = await _basketRepository.GetAll(a => a.UserId == userId && a.Status == DataAccess.Enums.Status.Created).FirstOrDefaultAsync();

                if (basket != null)
                {
                    basket.Status = DataAccess.Enums.Status.Paid;
                    basket.Payed = DateTime.Now;

                    await _basketRepository.Update(basket);

                    var basketItems = await _basketRepository.GetAllBasketItems(a => a.BasketId == basket.Id).ToListAsync();
                    foreach (var item in basketItems)
                    {
                        item.Status = DataAccess.Enums.Status.Removed;
                        await _basketRepository.UpdateBasketItems(item);
                    }

                    await _basketRepository.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false; // Handle and log errors as needed
            }
        }

        // New method to update the order
        public async Task<bool> UpdateOrder(AdmiOrderDto order)
        {
            var basket = await _basketRepository.GetAll(a => a.Id == order.Id).FirstOrDefaultAsync();
            if (basket != null)
            {
                basket.Address = order.Address;
                basket.Mobile = order.Mobile;
                basket.Status = DataAccess.Enums.Status.Created; // Just an example, adjust as per your logic

                await _basketRepository.Update(basket);
                return true;
            }
            return false;
        }

        // New method to delete the order
        public async Task DeleteOrder(int orderId)
        {
            var basket = await _basketRepository.GetAll(a => a.Id == orderId).FirstOrDefaultAsync();
            if (basket != null)
            {
                var basketItems = await _basketRepository.GetAllBasketItems(a => a.BasketId == basket.Id).ToListAsync();
                foreach (var item in basketItems)
                {
                    await _basketRepository.DeleteBasketItems(item);
                }
                await _basketRepository.Delete(basket);
                await _basketRepository.SaveChangesAsync();
            }
        }
    }
}
