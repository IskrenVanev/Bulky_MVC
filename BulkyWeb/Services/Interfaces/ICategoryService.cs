using BulkyBook.Models;

namespace BulkyBookWeb.Services
{
    public interface ICategoryService
    {
        IEnumerable<Category> GetAllCategories();
        Category? GetCategoryById(int id);
        string? ValidateCategory(Category category);
        void CreateCategory(Category category);
        void UpdateCategory(Category category);
        bool DeleteCategory(int id);
    }
}
