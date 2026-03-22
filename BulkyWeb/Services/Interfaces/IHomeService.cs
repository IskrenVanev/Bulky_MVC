using BulkyBook.Models;

namespace BulkyBookWeb.Services
{
    public interface IHomeService
    {
        IEnumerable<Product> GetAllProducts(string category = null);
        ShoppingCart GetProductDetails(int productId);
        void AddToCart(ShoppingCart shoppingCart, string userId);
        Task AddCommentAsync(string content, int productId, string userId, string userName, int rating);
    }
}
