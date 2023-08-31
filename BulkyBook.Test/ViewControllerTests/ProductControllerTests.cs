using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBookWeb.Areas.Admin.Controllers;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using BulkyBook.DataAccess.Repository;
using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;
using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace BulkyBook.Test.ViewControllerTests
{
    [TestFixture]
    public class ProductControllerTests
    {

        private Mock<IUnitOfWork> _unitOfWorkMock;
        private Mock<IProductRepository> _productRepositoryMock;
        private readonly IWebHostEnvironment _webHostEnvironment;


        //private Fixture _fixture;
        private ProductController _productController;
        private Mock<ProductVM> _productVMMock;

        [SetUp]
        public void Setup()
        {
            //_fixture = new Fixture();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            
            _productController = new ProductController(_unitOfWorkMock.Object, _webHostEnvironment);
            _productVMMock = new Mock<ProductVM>();
            
        }


        public void Index_View_Test()
        {
            List<Product> products = new List<Product>();
            _unitOfWorkMock.Setup(uow => uow.Product.GetAll(null, "Category")).Returns(products);


            var result = _productController.Index() as ViewResult;

            // Assert
            Assert.NotNull(result);

            Assert.IsInstanceOf<ViewResult>(result);
            Assert.IsAssignableFrom<List<Product>>(result.Model);

            Assert.AreEqual("Index", result.ViewName);

        }
        [Test]
        public void Upsert_Create_NewProduct_ReturnsViewWithCategories()
        {
            // Arrange
            int? id = null;
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            var mockCategoryRepository = new Mock<IRepository<Category>>();

            // Mock Category repository to return some dummy categories
            mockCategoryRepository.Setup(repo => repo.GetAll(It.IsAny<Expression<Func<Category, bool>>>(), It.IsAny<string>()))
                .Returns(new List<Category>
                {
                    new Category { Id = 1, Name = "Category 1" },
                    new Category { Id = 2, Name = "Category 2" }
                });

            mockUnitOfWork.Setup(uow => uow.Category.GetAll(
                    It.IsAny<Expression<Func<Category, bool>>>(),
                    It.IsAny<string>()
                )
            ).Returns(new List<Category>
            {
                new Category { Id = 1, Name = "Category 1" },
                new Category { Id = 2, Name = "Category 2" }
            });

            var controller = new ProductController(mockUnitOfWork.Object, _webHostEnvironment);

            // Act
            var result = controller.Upsert(id) as ViewResult;
            var model = result.Model as ProductVM;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Upsert", result.ViewName);
            Assert.IsNotNull(model);
            Assert.IsNotNull(model.CategoryList);
            Assert.AreEqual(2, model.CategoryList.Count());

        }

        [Test]
        public void Upsert_Update_ExistingProduct_ReturnsViewWithProduct()
        {

            // Arrange
            int? id = 1; // Existing product id
           
            var mockProductRepository = new Mock<IRepository<Product>>();
            var mockCategoryRepository = new Mock<IRepository<Category>>();

            // Mock Category repository to return some dummy categories
            
            _unitOfWorkMock.Setup(uow => uow.Category.GetAll(
       It.IsAny<Expression<Func<Category, bool>>>(),
       It.IsAny<string>()
   )
).Returns(new List<Category>
{
    new Category { Id = 1, Name = "Category 1" },
    new Category { Id = 2, Name = "Category 2" }
});
            var productMock = new Product
            {
                Id = 1,
                Title = "Product 1",
                Description = "Product description",
                ISBN = "1234567890",
                Author = "John Doe",
                ListPrice = 29.99,
                Price = 19.99,
                Price50 = 17.99,
                Price100 = 14.99,
                CategoryId = 2,
                Category = new Category { Id = 2, Name = "Category 2" },
                ProductImages = new List<ProductImage>
                {
                    new ProductImage(), // Simulate product images
                    new ProductImage()
                }
            };


            // Mock Product repository to return a dummy product
            _unitOfWorkMock.Setup(uow => uow.Product.Get(It.IsAny<Expression<Func<Product, bool>>>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>()
                ))
                .Returns(productMock);







            var controller = new ProductController(_unitOfWorkMock.Object, _webHostEnvironment);

            // Act
            var result = controller.Upsert(id) as ViewResult;
            var model = result.Model as ProductVM;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Upsert", result.ViewName);
            Assert.IsNotNull(model);
            Assert.IsNotNull(model.Product);
            Assert.AreEqual(1, model.Product.Id); // Verify it's the correct product
        }

        [Test]
        public void Upsert_ValidDataWithoutFiles_RedirectsToIndex()
        {
          

            var productVm = new ProductVM
            {
                Product = new Product
                {
                    Id = 0,
                    Title = "Sample Title",
                    Description = "Sample Description",
                    ISBN = "1234567890",
                    Author = "Sample Author",
                    ListPrice = 50.0,
                    Price = 40.0,
                    Price50 = 35.0,
                    Price100 = 30.0,
                    CategoryId = 1, // Assuming CategoryId for the related category
                    Category = new Category
                    {
                        // Initialize Category properties
                        Id = 1,
                        Name = "Sample Category",
                        // Set other properties of the Category
                    },
                    ProductImages = new List<ProductImage>
                    {
                        new ProductImage
                        {
                            Id = 1,
                            ImageUrl = "sample-image-1.jpg", // Sample image URL
                            ProductId = 0, // Assuming ProductId
                        },
                        new ProductImage
                        {
                            Id = 2,
                            ImageUrl = "sample-image-2.jpg", // Sample image URL
                            ProductId = 0, // Assuming ProductId
                        }
                        // You can add more images as needed
                    }
                },
                // Set the CategoryList property (assuming you have a list of SelectListItems)
                CategoryList = new List<SelectListItem>
                {
                    new SelectListItem { Value = "1", Text = "Category 1" },
                    new SelectListItem { Value = "2", Text = "Category 2" },
                    // Add more items as needed
                }
            };
            //var formFileMock = new Mock<IFormFile>();
            //formFileMock.Setup(f => f.FileName).Returns("test.jpg");
            var files = new List<IFormFile>();

            _unitOfWorkMock.Setup(uow => uow.Product.Update(It.IsAny<Product>()));
            _unitOfWorkMock.Setup(uow => uow.Save());

            
            var webRootPath = @"\..\..\BulkyBookWeb\wwwroot";


            var webHostEnvironmentMock = new Mock<IWebHostEnvironment>();

            webHostEnvironmentMock.Setup(env => env.WebRootPath).Returns(webRootPath); // Set the appropriate path
            var tempData = new Mock<ITempDataDictionary>();
            var productController = new ProductController(_unitOfWorkMock.Object, webHostEnvironmentMock.Object)
            {
                TempData = tempData.Object
            };
            var result = productController.Upsert(productVm, files) as RedirectToActionResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);

        }

        [Test]
        public void Upsert_ValidDataWithFiles_SavesProductAndFiles()
        {
            // Arrange

            var product = new Product
            {
                Id = 0,
                Title = "Sample Title",
                Description = "Sample Description",
                ISBN = "1234567890",
                Author = "Sample Author",
                ListPrice = 50.0,
                Price = 40.0,
                Price50 = 35.0,
                Price100 = 30.0,
                CategoryId = 1, // Assuming CategoryId for the related category
                Category = new Category
                {
                    // Initialize Category properties
                    Id = 1,
                    Name = "Sample Category",
                    // Set other properties of the Category
                },
                ProductImages = new List<ProductImage>
                {
                    new ProductImage
                    {
                        Id = 1,
                        ImageUrl = "sample-image-1.jpg", // Sample image URL
                        ProductId = 0, // Assuming ProductId


                    },
                    new ProductImage
                    {
                        Id = 2,
                        ImageUrl = "sample-image-2.jpg", // Sample image URL
                        ProductId = 0, // Assuming ProductId
                    }
                    // You can add more images as needed
                }
            };
            string wwwRootPath = @"\..\..\BulkyBookWeb\wwwroot";
            var webHostEnvironmentMock = new Mock<IWebHostEnvironment>();
            webHostEnvironmentMock.Setup(env => env.WebRootPath).Returns(wwwRootPath);
            var tempData = new Mock<ITempDataDictionary>();
            var controller = new ProductController(_unitOfWorkMock.Object, webHostEnvironmentMock.Object)
            {
                TempData = tempData.Object
            };
            var ProductVM= new ProductVM()
            {

            Product = product,
            // Set the CategoryList property (assuming you have a list of SelectListItems)
            CategoryList = new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "Category 1" },
                new SelectListItem { Value = "2", Text = "Category 2" },
                // Add more items as needed
            }

            };

        
            var formFileMock = new Mock<IFormFile>();
            formFileMock.Setup(f => f.FileName).Returns("test.jpg");
            var files = new List<IFormFile> { formFileMock.Object };

            // Set up expectations for unit of work methods
            _unitOfWorkMock.Setup(uow => uow.Product.Update(It.IsAny<Product>()));
            _unitOfWorkMock.Setup(uow => uow.Save());
            
            string productPath = @"images\products\product-" + ProductVM.Product.Id;
            string finalPath = Path.Combine(wwwRootPath, productPath);

           

            

            // Act

            var result = controller.Upsert(ProductVM, files) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);

            // Verify that product was updated and saved
            _unitOfWorkMock.Verify(uow => uow.Product.Update(product), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.Save(), Times.AtLeastOnce);

            // Verify that file-related operations were performed
            formFileMock.Verify(file => file.CopyTo(It.IsAny<Stream>()), Times.Once);
        }






        [Test]
        public void Upsert_InvalidData_ReturnsViewWithErrors()
        {
            List<Product> products = new List<Product>();
            var product = new Product
            {
                Id = 0,
                Title = "", // Set an empty string to violate the [Required] attribute
                Description = "Sample Description",
                ISBN = "1234567890",
                Author = "Sample Author",
                ListPrice = 50.0,
                Price = 40.0,
                Price50 = 35.0,
                Price100 = 30.0,
                CategoryId = 1,
                Category = new Category
                {
                    Id = 1,
                    Name = "Sample Category",
                },
                ProductImages = new List<ProductImage>
                {
                    new ProductImage
                    {
                        Id = 1,
                        ImageUrl = "sample-image-1.jpg",
                        ProductId = 0,
                    },
                    new ProductImage
                    {
                        Id = 2,
                        ImageUrl = "sample-image-2.jpg",
                        ProductId = 0,
                    }
                }
            };
            // Arrange
            var productVm = new ProductVM
            {
                Product = product,
                // Set the CategoryList property (assuming you have a list of SelectListItems)
                CategoryList = new List<SelectListItem>
                {
                    new SelectListItem { Value = "1", Text = "Category 1" },
                    new SelectListItem { Value = "2", Text = "Category 2" },
                    // Add more items as needed
                }


            };
            var categoryList = new List<Category>
            {
                new Category { Id = 1, Name = "Category 1" },
                new Category { Id = 2, Name = "Category 2" }
            };

            _unitOfWorkMock.Setup(uow => uow.Category.GetAll(null, null)).Returns(categoryList);
            products.Add(product);
            var webHostEnvironmentMock = new Mock<IWebHostEnvironment>();
            var tempData = new Mock<ITempDataDictionary>();
            var controller = new ProductController(_unitOfWorkMock.Object, webHostEnvironmentMock.Object)
            {
                TempData = tempData.Object
            };
            var formFileMock = new Mock<IFormFile>();
            formFileMock.Setup(f => f.FileName).Returns("test.jpg");
            var files = new List<IFormFile> { formFileMock.Object };
            // Add model state errors to simulate invalid data
            controller.ModelState.AddModelError("FieldKey", "ErrorMessage");
            _unitOfWorkMock.Setup(uow => uow.Product.GetAll(null, null)).Returns(products);
            // Act
            var result = controller.Upsert(productVm, files) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.ViewData.ModelState.IsValid); // Ensure model state is invalid
            Assert.AreEqual(null, result.ViewName); // Assuming an empty view name means the default view
        }
        [Test]
        public void DeleteImage_ValidImageId_DeletesImageAndRedirectsToUpsert()
        {
            // Arrange
            int imageIdToDelete = 1; // Replace with a valid image ID
            var imageToBeDeleted = new ProductImage
            {
                Id = imageIdToDelete,
                ImageUrl = "sample-image.jpg", // Set the image URL accordingly
                ProductId = 1 // Replace with a valid product ID
            };

            _unitOfWorkMock.Setup(uow => uow.ProductImage.Get(
                    It.IsAny<Expression<Func<ProductImage, bool>>>(),
                    null, // includeProperties
                    false  // tracked
                ))
                .Returns(imageToBeDeleted);
            string wwwRootPath = @"\..\..\BulkyBookWeb\wwwroot";
            var webHostEnvironmentMock = new Mock<IWebHostEnvironment>();

            webHostEnvironmentMock.Setup(env => env.WebRootPath).Returns(wwwRootPath); // Replace with the actual path

            var tempData = new Mock<ITempDataDictionary>();
            var controller = new ProductController(_unitOfWorkMock.Object, webHostEnvironmentMock.Object)
            {
                TempData = tempData.Object
            };


            //Act
            var result = controller.DeleteImage(imageIdToDelete) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Upsert", result.ActionName);
            Assert.AreEqual(imageToBeDeleted.ProductId, result.RouteValues["id"]);

            // Verify that the image was deleted and Save was called
            _unitOfWorkMock.Verify(uow => uow.ProductImage.Remove(imageToBeDeleted), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.Save(), Times.Once);

        }

        [Test]
        public void GetAll_ReturnsJsonResultWithData()
        {
            // Arrange
            var productList = new List<Product>
            {
                new Product { Id = 1, Title = "Product 1", Category = new Category { Id = 1, Name = "Category 1" } },
                new Product { Id = 2, Title = "Product 2", Category = new Category { Id = 2, Name = "Category 2" } }
            };

            _unitOfWorkMock.Setup(uow => uow.Product.GetAll(null, "Category"))
                .Returns(productList);

            var controller = new ProductController(_unitOfWorkMock.Object, _webHostEnvironment);

            // Act
            var result = controller.GetAll() as JsonResult;
           // var data = result.Value as { data: List<Product>};

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);

        
        }


        [Test]
        public void Delete_ProductExists_DeletesProductAndReturnsSuccessJson()
        {
            // Arrange
            int productIdToDelete = 1; // Replace with a valid product ID
            var productToBeDeleted = new Product
            {
                Id = productIdToDelete,
                Title = "Product to be deleted",
                // Set other properties as needed
            };

            _unitOfWorkMock.Setup(uow => uow.Product.Get(
                    It.IsAny<Expression<Func<Product, bool>>>(),
                    null, // includeProperties
                    false  // tracked
                ))
                .Returns(productToBeDeleted);
            string wwwRootPath = @"\..\..\BulkyBookWeb\wwwroot";
            var webHostEnvironmentMock = new Mock<IWebHostEnvironment>();
            webHostEnvironmentMock.Setup(env => env.WebRootPath).Returns(wwwRootPath); // Replace with the actual path

            var controller = new ProductController(_unitOfWorkMock.Object, webHostEnvironmentMock.Object);



            // Act
            var result = controller.Delete(productIdToDelete) as JsonResult;
            //var jsonData = result.Value as Dictionary<string, object>;




            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<JsonResult>(result);
            Assert.That(result.Value, Is.Not.Null, "success");
            Assert.That(result.Value, Is.Not.Null, "Delete successful");
           

            // Verify that the product was deleted and Save was called
            _unitOfWorkMock.Verify(uow => uow.Product.Remove(productToBeDeleted), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.Save(), Times.Once);
        }


        [Test]
        public void Delete_ProductNotFound_ReturnsErrorJson()
        {
            // Arrange
            int nonExistentProductId = 999; // Replace with a non-existent product ID

            _unitOfWorkMock.Setup(uow => uow.Product.Get(
                    It.IsAny<Expression<Func<Product, bool>>>(),
                    null, // includeProperties
                    false  // tracked
                ))
                .Returns((Product)null); // Return null to simulate product not found

            var webHostEnvironmentMock = new Mock<IWebHostEnvironment>();
            var controller = new ProductController(_unitOfWorkMock.Object, webHostEnvironmentMock.Object);

            // Act
            var result = controller.Delete(nonExistentProductId) as JsonResult;
            

            // Assert
            Assert.IsNotNull(result);
            

            // Verify that no deletion or Save was performed
            _unitOfWorkMock.Verify(uow => uow.Product.Remove(It.IsAny<Product>()), Times.Never);
            _unitOfWorkMock.Verify(uow => uow.Save(), Times.Never);
        }


    }
}
