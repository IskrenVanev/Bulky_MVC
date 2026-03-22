using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Product = BulkyBook.Models.Product;

namespace BulkyBookWeb.Services
{
    public interface IProductService
    {
        IEnumerable<Product> GetAllProducts();
        ProductVM GetProductVmForUpsert(int? id);
        void UpsertProduct(ProductVM productVm, List<IFormFile>? files);
        int? DeleteImage(int imageId);
        bool DeleteProduct(int id);
    }
}
