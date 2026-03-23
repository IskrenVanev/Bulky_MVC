using BulkyBook.Models;
using BulkyBook.Models.ViewModels;

namespace BulkyBookWeb.Services
{
    public interface ICartService
    {
        ShoppingCartVM GetShoppingCart(string userId);
        ShoppingCartVM GetSummary(string userId);
        int PrepareOrder(ShoppingCartVM shoppingCartVM, string userId);
        string CreateStripeSession(int orderHeaderId, string userId, string domain);
        void ProcessOrderConfirmation(int orderHeaderId);
        int Plus(int cartId);
        int Minus(int cartId);
        void Remove(int cartId);
        int GetCartCount(string userId);
    }
}
