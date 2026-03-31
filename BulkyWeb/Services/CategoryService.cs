using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using StackExchange.Redis;
using System.Text.Json;

namespace BulkyBookWeb.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDatabase _cache;
        private const string CategoryCacheKey = "AllCategories";

        public CategoryService(IUnitOfWork unitOfWork, IConnectionMultiplexer redis)
        {
            _unitOfWork = unitOfWork;
            _cache = redis.GetDatabase();
        }

        public IEnumerable<Category> GetAllCategories()
        {
            var cachedCategories = _cache.StringGet(CategoryCacheKey);
            if (!cachedCategories.IsNull)
            {
                return JsonSerializer.Deserialize<IEnumerable<Category>>(cachedCategories!) ?? Enumerable.Empty<Category>();
            }

            var categories = _unitOfWork.Category.GetAll().ToList();
            _cache.StringSet(CategoryCacheKey, JsonSerializer.Serialize(categories), TimeSpan.FromMinutes(10));

            return categories;
        }

        public Category? GetCategoryById(int id)
        {
            string cacheKey = $"Category_{id}";
            var cachedCategory = _cache.StringGet(cacheKey);
            if (!cachedCategory.IsNull)
            {
                return JsonSerializer.Deserialize<Category>(cachedCategory!);
            }

            var category = _unitOfWork.Category.Get(u => u.Id == id);
            if (category != null)
            {
                _cache.StringSet(cacheKey, JsonSerializer.Serialize(category), TimeSpan.FromMinutes(10));
            }

            return category;
        }

        public string? ValidateCategory(Category category)
        {
            if (category.Name == category.DisplayOrder.ToString())
            {
                return "The DisplayOrder cannot exactly match the Name.";
            }

            return null;
        }

        public void CreateCategory(Category category)
        {
            _unitOfWork.Category.Add(category);
            _unitOfWork.Save();
            InvalidateCache();
        }

        public void UpdateCategory(Category category)
        {
            _unitOfWork.Category.Update(category);
            _unitOfWork.Save();
            InvalidateCache(category.Id);
        }

        public bool DeleteCategory(int id)
        {
            Category? category = _unitOfWork.Category.Get(u => u.Id == id);
            if (category == null)
            {
                return false;
            }

            _unitOfWork.Category.Remove(category);
            _unitOfWork.Save();
            InvalidateCache(id);
            return true;
        }

        private void InvalidateCache(int? id = null)
        {
            _cache.KeyDelete(CategoryCacheKey);
            if (id.HasValue)
            {
                _cache.KeyDelete($"Category_{id.Value}");
            }
        }
    }
}
