using BulkyBook.Models;
using BulkyBook.Utility;
using BulkyBookWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public IActionResult Index()
        {
            List<Category> objCategoryList = _categoryService.GetAllCategories().ToList();
            return View("Index",objCategoryList);
        }

        public IActionResult Create()
        {
            return View("Create");
        }

        [HttpPost]
        public IActionResult Create(Category obj)
        {
            string? validationError = _categoryService.ValidateCategory(obj);
            if (!string.IsNullOrWhiteSpace(validationError))
            {
                ModelState.AddModelError("name", validationError);
            }

            if (ModelState.IsValid)
            {
                _categoryService.CreateCategory(obj);
                TempData["success"] = "Category created successfully";
                return RedirectToAction("Index");
            }
            return View("Create");
        }

        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Category? categoryFromDb = _categoryService.GetCategoryById(id.Value);
            if (categoryFromDb == null)
            {
                return NotFound();
            }
            return View("Edit",categoryFromDb);
        }

        [HttpPost]
        public IActionResult Edit(Category obj)
        {


            if (ModelState.IsValid)
            {
                _categoryService.UpdateCategory(obj);

                TempData["success"] = "Category updated successfully";
                return RedirectToAction("Index");
            }
            return View();

        }

        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Category? categoryFromDb = _categoryService.GetCategoryById(id.Value);
            //Category? categoryFromDb1 = _db.Categories.FirstOrDefault(u => u.Id == id);
            //Category? categoryFromDb2 = _db.Categories.Where(u=>u.Id == id).FirstOrDefault();
            if (categoryFromDb == null)
            {
                return NotFound();
            }
            return View("Delete", categoryFromDb);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePOST(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            bool isDeleted = _categoryService.DeleteCategory(id.Value);
            if (!isDeleted)
            {
                return NotFound();
            }

            TempData["success"] = "Category deleted successfully";
            return RedirectToAction("Index");
        }
    }
}