using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;

namespace BulkyBookWeb.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<Category> GetAllCategories()
        {
            return _unitOfWork.Category.GetAll();
        }

        public Category? GetCategoryById(int id)
        {
            return _unitOfWork.Category.Get(u => u.Id == id);
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
        }

        public void UpdateCategory(Category category)
        {
            _unitOfWork.Category.Update(category);
            _unitOfWork.Save();
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
            return true;
        }
    }
}
