using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Utility;

namespace BulkyBookWeb.Services
{
    public class HomeService : IHomeService
    {
        private readonly IUnitOfWork _unitOfWork;

        public HomeService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<Product> GetAllProducts(string category = null)
        {
            IEnumerable<Product> productList = _unitOfWork.Product.GetAll(includeProperties: "Category,ProductImages");
            
            if (!string.IsNullOrEmpty(category) && !category.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                productList = productList.Where(u => u.Category.Name.Equals(category, StringComparison.OrdinalIgnoreCase));
            }
            
            return productList;
        }

        public ShoppingCart GetProductDetails(int productId)
        {
            return new ShoppingCart
            {
                Product = _unitOfWork.Product.Get(u => u.Id == productId, includeProperties: "Category,ProductImages,Reviews.ApplicationUser"),
                Count = 1,
                ProductId = productId
            };
        }

        public void AddToCart(ShoppingCart shoppingCart, string userId)
        {
            shoppingCart.ApplicationUserId = userId;

            ShoppingCart cartFromDb = _unitOfWork.ShoppingCart.Get(u =>
                u.ApplicationUserId == userId && u.ProductId == shoppingCart.ProductId);

            if (cartFromDb != null)
            {
                cartFromDb.Count += shoppingCart.Count;
                _unitOfWork.ShoppingCart.Update(cartFromDb);
            }
            else
            {
                _unitOfWork.ShoppingCart.Add(shoppingCart);
            }
            _unitOfWork.Save();
        }

        public async Task AddCommentAsync(string content, int productId, string userId, string userName, int rating)
        {
            if (!string.IsNullOrEmpty(content))
            {
                await _unitOfWork.Review.AddReviewAsync(content, productId, userId, userName, rating);
            }
        }
    }
}
