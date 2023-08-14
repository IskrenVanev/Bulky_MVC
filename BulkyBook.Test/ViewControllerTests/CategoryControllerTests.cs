using NUnit.Framework;
using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBookWeb.Areas.Admin.Controllers;
using Microsoft.AspNetCore.Mvc;

using System.Collections.Generic;
using BulkyBook.Models;
using Moq;
using System.Diagnostics;
using System.Linq.Expressions;
using AutoFixture;
using Microsoft.AspNetCore.Identity;
using BulkyBook.DataAccess.Repository;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace BulkyBook.Test.ViewControllerTests
{
    [TestFixture]
    public class CategoryControllerTests
    {
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private Mock<ICategoryRepository> _categoryRepositoryMock;
        private Fixture _fixture;
        private CategoryController _categoryController;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _categoryController = new CategoryController(_unitOfWorkMock.Object);
        }

        [Test]
        public void Index_Test()
        {
            // Arrange
            var categories = new List<Category>(); // Mock list of categories
            _unitOfWorkMock.Setup(uow => uow.Category.GetAll(null, null)).Returns(categories);
            //var _categoryController = new CategoryController(_unitOfWorkMock.Object);
            // Act
            var result = _categoryController.Index() as ViewResult;

            // Assert
            Assert.NotNull(result);
            
            Assert.IsInstanceOf<ViewResult>(result);
            Assert.IsAssignableFrom<List<Category>>(result.Model);

            Assert.AreEqual("Index", result.ViewName);
        }
        [Test]
        public void Create_Test()
        {
            //var categoryController = new CategoryController(_unitOfWorkMock.Object);
            var result = _categoryController.Create() as ViewResult;

            Assert.NotNull(result); // Ensure the result is not null
            Assert.IsInstanceOf<ViewResult>(result); // Ensure the result 
            string actualViewName = result.ViewName; // Retrieve the actual view name

            Assert.AreEqual("Create", actualViewName);
        }

        [Test]
        public void CreatePOST_RedirectToIndex_Test()
        {
            var categoryObj = new Category
            {
                DisplayOrder = 5,
                Id = 22,
                Name = "Horror"
            };
            _unitOfWorkMock.Setup(repo => repo.Category.Add(categoryObj));
            _unitOfWorkMock.Setup(repo => repo.Save());


            var tempData = new Mock<ITempDataDictionary>();

            _categoryController = new CategoryController(_unitOfWorkMock.Object)
            {
                TempData = tempData.Object
            };


    // Act: Call the Create action
    var result = _categoryController.Create(categoryObj) as RedirectToActionResult;
    
    // Assert
    Assert.NotNull(result);
    Assert.AreEqual("Index", result.ActionName);



        }

        [Test]
        public void CreatePOST_AddToRepo_Test()
        {
            var categoryObj = new Category
            {
                DisplayOrder = 7,
                Id = 23,
                Name = "Horror2"
            };
            _unitOfWorkMock.Setup(repo => repo.Category.Add(categoryObj));
            _unitOfWorkMock.Setup(repo => repo.Save());
            var tempData = new Mock<ITempDataDictionary>();

            _categoryController = new CategoryController(_unitOfWorkMock.Object)
            {
                TempData = tempData.Object
            };
            var result = _categoryController.Create(categoryObj);
            // Assert

            // Verify that Category.Add was called with the expected category object
            _unitOfWorkMock.Verify(repo => repo.Category.Add(categoryObj), Times.Once);

            // Verify that Save was called
            _unitOfWorkMock.Verify(repo => repo.Save(), Times.Once);

        }
        [Test]
        public void Edit_View_Test()
        {
            var categoryObj = new Category
            {
                DisplayOrder = 7,
                Id = 23,
                Name = "Horror2"
            };
            
           
            _unitOfWorkMock.Setup(repo => repo.Category.Get(
                    It.IsAny<Expression<Func<Category, bool>>>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>()
                ))
                .Returns((Expression<Func<Category, bool>> filter, string includeProperties, bool tracked) =>
                {
                    if (filter.Compile().Invoke(categoryObj))
                    {
                        return categoryObj;
                    }
                    return null; // Or throw an exception
                });




            var tempData = new Mock<ITempDataDictionary>();

            _categoryController = new CategoryController(_unitOfWorkMock.Object)
            {
                TempData = tempData.Object
            };

            var categoryCreate = _categoryController.Create(categoryObj);




           

            var result = _categoryController.Edit(categoryObj.Id) as ViewResult;

            Assert.NotNull(result);
            Assert.AreEqual("Edit", result.ViewName);

        }


        [Test]
        public void EditPOST_RedirectToIndex_Test()
        {
            var categoryObj = new Category
            {
                DisplayOrder = 5,
                Id = 22,
                Name = "Horror"
            };
            _unitOfWorkMock.Setup(repo => repo.Category.Update(categoryObj));
            _unitOfWorkMock.Setup(repo => repo.Save());


            var tempData = new Mock<ITempDataDictionary>();

            _categoryController = new CategoryController(_unitOfWorkMock.Object)
            {
                TempData = tempData.Object
            };


            // Act: Call the Create action
            var result = _categoryController.Edit(categoryObj) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual("Index", result.ActionName);


        }
        [Test]
        public void EditPOST_EditSomething_Test()
        {
            var categoryObj = new Category
            {
                DisplayOrder = 7,
                Id = 22,
                Name = "Horror"
            };

            _unitOfWorkMock.Setup(repo => repo.Category.Update(categoryObj));
            _unitOfWorkMock.Setup(repo => repo.Save());


            var tempData = new Mock<ITempDataDictionary>();

            _categoryController = new CategoryController(_unitOfWorkMock.Object)
            {
                TempData = tempData.Object
            };


            // Act: Call the Create action
            var result = _categoryController.Edit(categoryObj);

            // Assert



            // Verify that Category.Add was called with the expected category object
            _unitOfWorkMock.Verify(repo => repo.Category.Update(categoryObj), Times.Once);

            // Verify that Save was called
            _unitOfWorkMock.Verify(repo => repo.Save(), Times.Once);
        }


        [Test]
        public void DeletePOST_View_Test()
        {
           
            var categoryObj = new Category
            {
                DisplayOrder = 5,
                Id = 22,
                Name = "Horror"
            };
            
            _unitOfWorkMock.Setup(repo => repo.Category.Get(
                    It.IsAny<Expression<Func<Category, bool>>>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>()
                ))
                .Returns((Expression<Func<Category, bool>> filter, string includeProperties, bool tracked) =>
                {
                    if (filter.Compile().Invoke(categoryObj))
                    {
                        return categoryObj;
                    }
                    return null; // Or throw an exception
                });


            var tempData = new Mock<ITempDataDictionary>();

            _categoryController = new CategoryController(_unitOfWorkMock.Object)
            {
                TempData = tempData.Object
            };


            // Act: Call the Create action
            var result = _categoryController.Delete(categoryObj.Id) as ViewResult;

            // Assert
            Assert.NotNull(result);

            Assert.IsInstanceOf<ViewResult>(result);
            

            Assert.AreEqual("Delete", result.ViewName);
            

        }
        [Test]
        public void DeletePOST_DeleteSomething_Test()
        {

            var categoryObj = new Category
            {
                DisplayOrder = 5,
                Id = 22,
                Name = "Horror"
            };

            _unitOfWorkMock.Setup(repo => repo.Category.Get(
                    It.IsAny<Expression<Func<Category, bool>>>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>()
                ))
                .Returns((Expression<Func<Category, bool>> filter, string includeProperties, bool tracked) =>
                {
                    if (filter.Compile().Invoke(categoryObj))
                    {
                        return categoryObj;
                    }
                    return null; // Or throw an exception
                });

            var tempData = new Mock<ITempDataDictionary>();

            _categoryController = new CategoryController(_unitOfWorkMock.Object)
            {
                TempData = tempData.Object
            };

            // Act: Call the DeletePOST action
            var result = _categoryController.DeletePOST(categoryObj.Id) as RedirectToActionResult;

            // Assert

            // Verify that Category.Remove was called with the expected category object
            _unitOfWorkMock.Verify(repo => repo.Category.Remove(categoryObj), Times.Once);

            // Verify that Save was called
            _unitOfWorkMock.Verify(repo => repo.Save(), Times.Once);

            // Assert the action result
            Assert.NotNull(result);
            Assert.AreEqual("Index", result.ActionName);


        }



    }
    
}


// Assert.AreEqual("Create", createdResult.ActionName);
// Assert.IsNotNull(createdResult.Value);



//var insertedObject = _unitOfWorkMock.Object.Category.Get(o => o.Name == TheName);
//Assert.IsNotNull(insertedObject);
//Assert.AreEqual(TheName, categoryObj.Name);
