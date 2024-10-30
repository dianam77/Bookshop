using Core.OrderService.Model;
using DataAccess.Enums;
using DataAccess.Models;
using DataAccess.Repositories.BasketRepo;
using DataAccess.Repositories.BookRepo;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.OrderService
{
    public class OrderService
    {
        private readonly IBasketRepository _basketRepository;
        private readonly IBookRepository _bookRepository;

        public object Orders { get; set; }

        public OrderService(IBasketRepository basketRepository, IBookRepository bookRepository)
        {
            _basketRepository = basketRepository;
            _bookRepository = bookRepository;
        }
        public async Task<bool> AddToBasket(int bookId, int qty, string userId)
        {
            var basket = new Basket();
            basket = await _basketRepository.GetAll(a => a.UserId == userId && a.Status == DataAccess.Enums.Status.Created).FirstOrDefaultAsync();

            if (basket == null)
            {
                basket = new Basket()
                {
                    UserId = userId,
                    Created = DateTime.Now,
                    Status = DataAccess.Enums.Status.Created,
                    Address = "",
                    Mobile = "",
                    Payed = DateTime.Now,
                };
                await _basketRepository.Add(basket);
            }
            var book = await _bookRepository.GetById(bookId);
            var BasketItem = new BasketItems()
            {
                BasketId = basket.Id,
                Qty = qty,
                BookId = book.Id,
                Created = DateTime.Now,
                Price = book.Price * qty
            };
            _basketRepository.AddBasketItems(BasketItem);
            return true;
        }
        public async Task<List<BasketItems>> GetUserBasket(string userId)
        {
            var baskets = await _basketRepository.GetAllBasketItems(a => a.Basket.UserId == userId && a.Basket.Status == DataAccess.Enums.Status.Created)
                .Include(a => a.Basket).Include(a=> a.Book).AsNoTracking().ToListAsync();

            return baskets;
        }

        public async Task<bool> RemoveItemBasket(int id)
        {
            var baskets = await _basketRepository.GetAllBasketItems(a => a.Id == id).FirstOrDefaultAsync();

            await _basketRepository.DeleteBasketItems(baskets);

            return true;
        }

        public async Task<bool> Pay(string address, string mobile , string userId)
        {
            var basket = await _basketRepository.GetAll(a => a.UserId == userId && a.Status == DataAccess.Enums.Status.Created).FirstOrDefaultAsync();
            if (basket == null)
                return false;
            basket.Mobile = mobile;
            basket.Address = address;
            basket.Payed = DateTime.Now;
            basket.Status = DataAccess.Enums.Status.Final;

            await _basketRepository.Update(basket);
            
            return true;
        }

        public async Task<List<Basket>> GetUserOrders(string userId)
        {
            var baskets = await _basketRepository.GetAll(a => a.UserId == userId && a.Status != DataAccess.Enums.Status.Created)
                .Include(a => a.BasketItems)
                .ThenInclude(a => a.Book).AsNoTracking().ToListAsync();
            return baskets;
        }

        public async Task<List<AdmiOrderDto>> GetAllOrders()
        {
            var baskets = await _basketRepository.GetAll(a => a.Status != DataAccess.Enums.Status.Created)
                .Include(a => a.User)
                .Include(a => a.BasketItems)
                .ThenInclude(a => a.Book)
                .Select(s => new AdmiOrderDto()
                {
                    Address = s.Address,
                    Id = s.Id,
                    Mobile = s.Mobile,
                    Status = s.Status,
                    Payed = s.Payed,
                    UserId = s.UserId,
                    UserName = s.User.UserName,
                    Items = s.BasketItems.Select(c => c.Book.Title).ToList()
                })
               .AsNoTracking().ToListAsync();
            return baskets;
        }
        public async Task<AdmiOrderDto> GetOrderWithId(int id)
        {
            var baskets = await _basketRepository.GetAll(a => a.Id== id)
                .Include(a => a.User)
                .Include(a => a.BasketItems)
                .ThenInclude(a => a.Book)
                .Select(s => new AdmiOrderDto()
                {
                    Address = s.Address,
                    Id = s.Id,
                    Mobile = s.Mobile,
                    Status = s.Status,
                    Payed = s.Payed,
                    UserId = s.UserId,
                    UserName = s.User.UserName,
                    Items = s.BasketItems.Select(c => c.Book.Title).ToList()
                })
               .AsNoTracking().FirstOrDefaultAsync();
            return baskets;
        }
        public async Task<bool> SetStatus(int Id, bool State)
        {
            var basket = await _basketRepository.GetAll(a => a.Id == Id).FirstOrDefaultAsync();
            if (State)
            {
                basket.Status = DataAccess.Enums.Status.Accepted;
            }
            else
            {
                basket.Status = DataAccess.Enums.Status.Rejected;
            }
            _basketRepository.Update(basket);
            return true;
        }

        public async Task<bool> MarkBasketItemsAsPaid(string userId)
        {
            try
            {
                // Fetch user's basket
                var basket = await _basketRepository.GetAll(a => a.UserId == userId && a.Status == DataAccess.Enums.Status.Created).FirstOrDefaultAsync();

                if (basket != null)
                {
                    // Mark basket status as paid
                    basket.Status = DataAccess.Enums.Status.Paid; // Change to your paid status
                    basket.Payed = DateTime.Now; // Optionally set the pay date

                    // Save changes to the basket
                    await _basketRepository.Update(basket);

                    // Optionally, remove items from the basket if required
                    var basketItems = await _basketRepository.GetAllBasketItems(a => a.BasketId == basket.Id).ToListAsync();
                    foreach (var item in basketItems)
                    {
                        item.Status = DataAccess.Enums.Status.Removed; // Set item status to removed
                        await _basketRepository.UpdateBasketItems(item); // Update the item status
                    }

                    await _basketRepository.SaveChangesAsync(); // Save all changes
                    return true;
                }
                return false;
            }
            catch (Exception)
            {

                return false;
            }
        }

    }

}
    


