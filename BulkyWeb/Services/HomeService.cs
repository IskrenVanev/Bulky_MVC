using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Utility;
using StackExchange.Redis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BulkyBookWeb.Services
{
    public class HomeService : IHomeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDatabase _cache;
        private const string HomeProductsCacheKey = "HomeProducts_";
        private readonly JsonSerializerOptions _jsonOptions;

        public HomeService(IUnitOfWork unitOfWork, IConnectionMultiplexer redis)
        {
            _unitOfWork = unitOfWork;
            _cache = redis.GetDatabase();
            _jsonOptions = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                PropertyNameCaseInsensitive = true
            };
        }

        public IEnumerable<Product> GetAllProducts(string category = null)
        {
            string categoryKey = category ?? "all";
            string cacheKey = HomeProductsCacheKey + categoryKey;

            var cachedData = _cache.StringGet(cacheKey);
            if (!cachedData.IsNull)
            {
                return JsonSerializer.Deserialize<IEnumerable<Product>>(cachedData!, _jsonOptions) ?? Enumerable.Empty<Product>();
            }

            IEnumerable<Product> productList = _unitOfWork.Product.GetAll(includeProperties: "Category,ProductImages");
            
            if (!string.IsNullOrEmpty(category) && !category.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                productList = productList.Where(u => u.Category.Name.Equals(category, StringComparison.OrdinalIgnoreCase));
            }

            var result = productList.ToList();
            _cache.StringSet(cacheKey, JsonSerializer.Serialize(result, _jsonOptions), TimeSpan.FromMinutes(5));
            
            return result;
        }

        public ShoppingCart GetProductDetails(int productId)
        {
            string cacheKey = $"HomeProductDetails_{productId}";
            var cachedData = _cache.StringGet(cacheKey);
            if (!cachedData.IsNull)
            {
                return JsonSerializer.Deserialize<ShoppingCart>(cachedData!, _jsonOptions)!;
            }

            var cart = new ShoppingCart
            {
                Product = _unitOfWork.Product.Get(u => u.Id == productId, includeProperties: "Category,ProductImages,Reviews.ApplicationUser"),
                Count = 1,
                ProductId = productId
            };

            _cache.StringSet(cacheKey, JsonSerializer.Serialize(cart, _jsonOptions), TimeSpan.FromMinutes(5));

            return cart;
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
                // Invalidate cart count cache
                _cache.KeyDelete("CartCount_" + userId);
            }
            _unitOfWork.Save();
        }

        public async Task AddCommentAsync(string content, int productId, string userId, string userName, int rating)
        {
            if (!string.IsNullOrEmpty(content))
            {
                await _unitOfWork.Review.AddReviewAsync(content, productId, userId, userName, rating);
                // Invalidate product details cache to show new comment
                _cache.KeyDelete($"HomeProductDetails_{productId}");
            }
        }
    }
}
