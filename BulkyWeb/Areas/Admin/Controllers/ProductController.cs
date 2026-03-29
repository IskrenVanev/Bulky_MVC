using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using BulkyBookWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Product = BulkyBook.Models.Product;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        public ProductController(IProductService productService)
        {
            _productService = productService;
        }
        public IActionResult Index()
        {
            List<Product> objProductList = _productService.GetAllProducts().ToList();

            return View(objProductList);
        }

        public IActionResult Upsert(int? id)
        {
            ProductVM productVm = _productService.GetProductVmForUpsert(id);
            if (id == null || id == 0)
            {
                //create
                return View("Upsert", productVm);
            }
            else
            {
                //update
                return View("Upsert", productVm);
            }
        }

        [HttpPost]
        public IActionResult Upsert(ProductVM productVm, List<IFormFile>? files)
        {
            ModelState.Remove("Reviews");
            if (ModelState.IsValid )
            {
                _productService.UpsertProduct(productVm, files);
                TempData["success"] = "Product created/updated successfully";
                return RedirectToAction("Index");
            }
            else
            {
                ProductVM rebuiltVm = _productService.GetProductVmForUpsert(productVm.Product.Id);
                rebuiltVm.Product = productVm.Product;
                return View(rebuiltVm);
            }
        }

        public IActionResult DeleteImage(int imageId)
        {
            int? productId = _productService.DeleteImage(imageId);
            if (productId.HasValue)
            {
                TempData["success"] = "Deleted successfully";
            }
            return RedirectToAction(nameof(Upsert), new{id= productId});
        }
        
        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            List<Product> objProductList = _productService.GetAllProducts().ToList();
            return Json(new { data = objProductList });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            bool isDeleted = _productService.DeleteProduct(id.Value);
            if (!isDeleted)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            return Json(new { success = true, message = "Delete successful" });
        }
        #endregion
    }
}