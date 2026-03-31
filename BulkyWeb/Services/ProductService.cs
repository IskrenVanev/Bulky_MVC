using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging.Abstractions;
using StackExchange.Redis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Product = BulkyBook.Models.Product;

namespace BulkyBookWeb.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<ProductService> _logger;
        private readonly IDatabase _cache;
        private const string ProductCacheKey = "AllProducts";
        private readonly JsonSerializerOptions _jsonOptions;

        public ProductService(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment, IConnectionMultiplexer redis, ILogger<ProductService>? logger = null)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger ?? NullLogger<ProductService>.Instance;
            _cache = redis.GetDatabase();
            _jsonOptions = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                PropertyNameCaseInsensitive = true
            };
        }

        public IEnumerable<Product> GetAllProducts()
        {
            var cachedProducts = _cache.StringGet(ProductCacheKey);
            if (!cachedProducts.IsNull)
            {
                return JsonSerializer.Deserialize<IEnumerable<Product>>(cachedProducts!, _jsonOptions) ?? Enumerable.Empty<Product>();
            }

            var products = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
            _cache.StringSet(ProductCacheKey, JsonSerializer.Serialize(products, _jsonOptions), TimeSpan.FromMinutes(10));

            return products;
        }

        public ProductVM GetProductVmForUpsert(int? id)
        {
            ProductVM productVm = new()
            {
                CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                Product = new Product()
            };

            if (id != null && id != 0)
            {
                string cacheKey = $"Product_{id}";
                var cachedProduct = _cache.StringGet(cacheKey);
                if (!cachedProduct.IsNull)
                {
                    productVm.Product = JsonSerializer.Deserialize<Product>(cachedProduct!, _jsonOptions)!;
                }
                else
                {
                    Product? existingProduct = _unitOfWork.Product.Get(u => u.Id == id, includeProperties: "ProductImages,Reviews");
                    if (existingProduct != null)
                    {
                        productVm.Product = existingProduct;
                        _cache.StringSet(cacheKey, JsonSerializer.Serialize(existingProduct, _jsonOptions), TimeSpan.FromMinutes(10));
                    }
                }
            }

            return productVm;
        }

        public void UpsertProduct(ProductVM productVm, List<IFormFile>? files)
        {
            bool isCreate = productVm.Product.Id == 0;

            if (productVm.Product.Id == 0)
            {
                _unitOfWork.Product.Add(productVm.Product);
            }
            else
            {
                _unitOfWork.Product.Update(productVm.Product);
            }
            _unitOfWork.Save();

            string wwwRootPath = _webHostEnvironment.WebRootPath;
            if (files != null)
            {
                foreach (var file in files)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = @"images\products\product-" + productVm.Product.Id;
                    string finalPath = Path.Combine(wwwRootPath, productPath);

                    if (!Directory.Exists(finalPath))
                    {
                        Directory.CreateDirectory(finalPath);
                    }
                    using (var fileStream = new FileStream(Path.Combine(finalPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    ProductImage productImage = new()
                    {
                        ImageUrl = @"\" + productPath + @"\" + fileName,
                        ProductId = productVm.Product.Id,

                    };
                    if (productVm.Product.ProductImages == null)
                    {
                        productVm.Product.ProductImages = new List<ProductImage>();
                    }

                    productVm.Product.ProductImages.Add(productImage);
                }
                _unitOfWork.Product.Update(productVm.Product);
                _unitOfWork.Save();
            }

            InvalidateCache(productVm.Product.Id);

            _logger.LogInformation(
                "Business event {EventType}: Product upsert completed. ProductId={ProductId}, Title={Title}, CategoryId={CategoryId}, UploadedImages={UploadedImages}",
                isCreate ? "ProductCreated" : "ProductUpdated",
                productVm.Product.Id,
                productVm.Product.Title,
                productVm.Product.CategoryId,
                files?.Count ?? 0);
        }

        public int? DeleteImage(int imageId)
        {
            ProductImage? imageToBeDeleted = _unitOfWork.ProductImage.Get(u => u.Id == imageId);
            if (imageToBeDeleted == null)
            {
                return null;
            }

            int productId = imageToBeDeleted.ProductId;
            if (!string.IsNullOrEmpty(imageToBeDeleted.ImageUrl))
            {
                var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, imageToBeDeleted.ImageUrl.TrimStart('\\'));
                if (File.Exists(oldImagePath))
                {
                    File.Delete(oldImagePath);
                }
            }

            _unitOfWork.ProductImage.Remove(imageToBeDeleted);
            _unitOfWork.Save();
            InvalidateCache(productId);

            return productId;
        }

        public bool DeleteProduct(int id)
        {
            Product? productToBeDeleted = _unitOfWork.Product.Get(u => u.Id == id);
            if (productToBeDeleted == null)
            {
                return false;
            }

            string productPath = @"images\products\product-" + id;
            string finalPath = Path.Combine(_webHostEnvironment.WebRootPath, productPath);

            if (Directory.Exists(finalPath))
            {
                string[] filePaths = Directory.GetFiles(finalPath);
                foreach (string filePath in filePaths)
                {
                    File.Delete(filePath);
                }
                Directory.Delete(finalPath);
            }

            _unitOfWork.Product.Remove(productToBeDeleted);
            _unitOfWork.Save();
            InvalidateCache(id);
            return true;
        }

        private void InvalidateCache(int? id = null)
        {
            _cache.KeyDelete(ProductCacheKey);
            if (id.HasValue)
            {
                _cache.KeyDelete($"Product_{id.Value}");
            }
        }
    }
}
