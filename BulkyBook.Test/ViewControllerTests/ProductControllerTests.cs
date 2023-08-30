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


      

    }
}
