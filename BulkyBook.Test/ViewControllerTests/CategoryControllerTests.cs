using NUnit.Framework;
using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBookWeb.Areas.Admin.Controllers;
using Microsoft.AspNetCore.Mvc;

using System.Collections.Generic;
using BulkyBook.Models;
using Moq;
using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using BulkyBook.DataAccess.Repository;

namespace BulkyBook.Test.ViewControllerTests
{
    [TestFixture]
    public class CategoryControllerTests
    {
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private CategoryController _categoryController;

        [SetUp]
        public void Setup()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _categoryController = new CategoryController(_unitOfWorkMock.Object);
        }

        [Test]
        public void Index_Test()
        {
            // Arrange
            var categories = new List<Category>(); // Mock list of categories
            _unitOfWorkMock.Setup(uow => uow.Category.GetAll(null, null)).Returns(categories);

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
            var categoryController = new CategoryController(_unitOfWorkMock.Object);
            var result = categoryController.Create() as ViewResult;

            Assert.NotNull(result); // Ensure the result is not null
            Assert.IsInstanceOf<ViewResult>(result); // Ensure the result 
            string actualViewName = result.ViewName; // Retrieve the actual view name

            Assert.AreEqual("Create", actualViewName);
        }

        [Test]
        public void CreatePOST_Test()
        {
            
            
            var categoryObj = new Category
            {
                Id = 5,
                Name = "Horror",
                DisplayOrder = 7
            };
            _unitOfWorkMock.Object.Category.Add(categoryObj);
            _unitOfWorkMock.Object.Save();


            

            // _unitOfWorkMock.Verify(uow => uow.Category.Add(It.IsAny<Category>()), Times.Once);
    //_unitOfWorkMock.Verify(uow => uow.Save(), Times.Once);
           

        }
    }
}


// Assert.AreEqual("Create", createdResult.ActionName);
// Assert.IsNotNull(createdResult.Value);



//var insertedObject = _unitOfWorkMock.Object.Category.Get(o => o.Name == TheName);
//Assert.IsNotNull(insertedObject);
//Assert.AreEqual(TheName, categoryObj.Name);
