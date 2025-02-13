using Core.OrderService.Model;
using DataAccess.Models;

namespace Core.OrderService
{
    public interface IOrderService
    {
        Task<bool> AddToBasket(int bookId, int qty, string userId);
        Task<List<BasketItems?>> GetUserBasket(string userId);
        Task<bool> RemoveItemBasket(int id);
        Task<bool> Pay(string address, string mobile, string userId);
        Task<List<Basket?>> GetUserOrders(string userId);
        Task<List<AdmiOrderDto>> GetAllOrders();
        Task<AdmiOrderDto?> GetOrderWithId(int id);
        Task<bool> SetStatus(int id, bool state);
        Task DeleteOrder(int orderId);  // Ensure DeleteOrder method is defined here
        Task<bool> UpdateOrder(AdmiOrderDto order); // Define UpdateOrder method here
        Task<bool> MarkBasketItemsAsPaid(string userId);
    }
}
