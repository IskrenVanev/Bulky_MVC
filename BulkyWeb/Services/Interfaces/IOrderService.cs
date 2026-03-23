using BulkyBook.Models;
using BulkyBook.Models.ViewModels;

namespace BulkyBookWeb.Services
{
    public interface IOrderService
    {
        OrderVM? GetOrderDetails(int orderId);
        int? UpdateOrderDetail(OrderHeader inputOrderHeader);
        bool StartProcessing(int orderId);
        bool ShipOrder(OrderHeader inputOrderHeader);
        bool CancelOrder(int orderId);
        string? CreateStripeSession(int orderId, string domain);
        void ConfirmPayment(int orderHeaderId);
        IEnumerable<OrderHeader> GetOrders(string? status, bool isPrivilegedUser, string? userId);
    }
}
