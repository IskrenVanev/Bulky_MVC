using AutoFixture;
using BulkyBook.DataAccess.Repository;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBookWeb.Areas.Admin.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BulkyBook.Test.ViewControllerTests
{
    [TestFixture]
    public class CompanyControllerTests
    {
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private Mock<ICompanyRepository> _companyRepositoryMock;
        //private Fixture _fixture;
        private CompanyController _companyController;

        [SetUp]
        public void Setup()
        {
            //_fixture = new Fixture();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _companyController = new CompanyController(_unitOfWorkMock.Object);

        }

        [Test]
        public void Index_View_Test()
        {


            // Arrange
            var objCompanyList = new List<Company>(); // Mock list of companies
            _unitOfWorkMock.Setup(uow => uow.Company.GetAll(null, null)).Returns(objCompanyList);

            // Act
            var result = _companyController.Index() as ViewResult;

            // Assert
            Assert.NotNull(result);

            Assert.IsInstanceOf<ViewResult>(result);
            Assert.IsAssignableFrom<List<Company>>(result.Model);

            Assert.AreEqual("Index", result.ViewName);

        }

        [Test]
        public void Upsert_ViewCreate_Test()
        {
            // Arrange
            int? id = null;

            // Act
            IActionResult result = _companyController.Upsert(id);

            // Assert
            Assert.IsInstanceOf<ViewResult>(result);
            ViewResult viewResult = result as ViewResult;
            Assert.IsInstanceOf<Company>(viewResult.Model);


        }

        [Test]
        public void Upsert_Update_Test()
        {
            int id = 12;
            var mockCompanyObj = new Company
            {
                City = "Pleven",
                Id = id,
                PhoneNumber = 123431234.ToString(),
                Name = "BiGCompAny",
                PostalCode = 1231.ToString(),
                State = "ASDF",
                StreetAddress = "Str. Dolno Nanagornishte"

            };
            _unitOfWorkMock.Setup(uow => uow.Company.Get(
                    It.IsAny<Expression<Func<Company, bool>>>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>()))
                .Returns(mockCompanyObj);

            var result = _companyController.Upsert(id);



            // Assert
            Assert.IsInstanceOf<ViewResult>(result);
            ViewResult viewResult = result as ViewResult;
            Assert.AreEqual(mockCompanyObj, viewResult.Model);
        }

        [Test]
        public void Upsert_Post_ValidModel_RedirectToIndex_Test()
        {
            var mockCompanyObj = new Company
            {
                City = "Pleven",
                Id = 12,
                PhoneNumber = 123431234.ToString(),
                Name = "BiGCompAny",
                PostalCode = 1231.ToString(),
                State = "ASDF",
                StreetAddress = "Str. Dolno Nanagornishte"

            };
            _unitOfWorkMock.Setup(repo => repo.Company.Update(mockCompanyObj));
            _unitOfWorkMock.Setup(repo => repo.Save());
            var tempData = new Mock<ITempDataDictionary>();

            _companyController = new CompanyController(_unitOfWorkMock.Object)
            {
                TempData = tempData.Object
            };
            var result = _companyController.Upsert(mockCompanyObj);




            Assert.IsInstanceOf<RedirectToActionResult>(result);
            RedirectToActionResult redirectResult = result as RedirectToActionResult;
            Assert.AreEqual("Index", redirectResult.ActionName);
            tempData.VerifySet(td => td["success"] = "Company created successfully");
        }

        [Test]
        public void Upsert_Post_InvalidModel_ReturnsView_Test()
        {
            // Arrange
            Company invalidCompanyObj = new Company
            {
                // No Name property set, which violates the [Required] attribute
                StreetAddress = "Invalid Address",
                City = "Invalid City",
                State = "Invalid State",
                PostalCode = "Invalid Postal Code",
                PhoneNumber = "Invalid Phone Number"
            };

            _companyController.ModelState.AddModelError("Name", "Name is required"); // Simulating ModelState error

            // Act
            IActionResult result = _companyController.Upsert(invalidCompanyObj);

            // Assert
            Assert.IsInstanceOf<ViewResult>(result);
            ViewResult viewResult = result as ViewResult;
            Assert.AreEqual("Upsert", viewResult.ViewName);
            Assert.AreEqual(invalidCompanyObj, viewResult.Model);
        }
        [Test]
        public void GetAll_ReturnsJsonData_Test()
        {
            // Arrange
            var mockCompanyList = new List<Company>
            {
               new Company
               {
                   City = "Pleven",
                   Id = 12,
                   PhoneNumber = 123431234.ToString(),
                   Name = "BiGCompAny",
                   PostalCode = 1231.ToString(),
                   State = "ASDF",
                   StreetAddress = "Str. Dolno Nanagornishte"
               },
                new Company
               {
                   City = "Ruse",
                   Id = 14,
                   PhoneNumber = 123431234.ToString(),
                   Name = "BiGCompAny2",
                   PostalCode = 1231.ToString(),
                   State = "ASDFG",
                   StreetAddress = "Str. Dolno Nanagornishte2"
               }




            
            };

            _unitOfWorkMock.Setup(repo => repo.Company.GetAll(
                    It.IsAny<Expression<Func<Company, bool>>>(),
                    It.IsAny<string>()))
                .Returns(mockCompanyList);


            // Act
            IActionResult result = _companyController.GetAll();

            // Assert
            Assert.IsInstanceOf<JsonResult>(result);
            JsonResult jsonResult = result as JsonResult;

            // Assert the JSON data content
            var jsonData = jsonResult.Value;
            var dataProperty = jsonData.GetType().GetProperty("data").GetValue(jsonData, null);

            Assert.IsAssignableFrom<List<Company>>(dataProperty);

            var serializedExpected = JsonSerializer.Serialize(mockCompanyList);
            var serializedActual = JsonSerializer.Serialize(dataProperty);

            Assert.AreEqual(serializedExpected, serializedActual);
        }


        [Test]
        public void Delete_Test()
        {
            int id = 12;
            var mockCompanyObj = new Company
            {
                City = "Pleven",
                Id = id,
                PhoneNumber = 123431234.ToString(),
                Name = "BiGCompAny",
                PostalCode = 1231.ToString(),
                State = "ASDF",
                StreetAddress = "Str. Dolno Nanagornishte"

            };
            _unitOfWorkMock.Setup(uow => uow.Company.Get(
                    It.IsAny<Expression<Func<Company, bool>>>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>()))
            .Returns(mockCompanyObj);

            IActionResult result = _companyController.Delete(id);

            // Assert
            Assert.IsInstanceOf<JsonResult>(result);

            JsonResult jsonResult = result as JsonResult;

            // Assert the JSON data content
            var jsonData = jsonResult.Value;
            var jObject = JObject.FromObject(jsonData);

            Assert.AreEqual(true, jObject["success"].Value<bool>());
            Assert.AreEqual("Delete successful", jObject["message"].Value<string>());

            _unitOfWorkMock.Verify(repo => repo.Company.Remove(mockCompanyObj), Times.Once);
            _unitOfWorkMock.Verify(repo => repo.Save(), Times.Once);

            
        }

        [Test]
        public void Delete_FailedDelete_ReturnsErrorJson()
        {
            int id = 999; // Assuming this ID does not exist
            _unitOfWorkMock.Setup(uow => uow.Company.Get(
                    It.IsAny<Expression<Func<Company, bool>>>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>()))
                .Returns((Company)null);

            IActionResult result = _companyController.Delete(id);

            // Assert
            Assert.IsInstanceOf<JsonResult>(result);

            JsonResult jsonResult = result as JsonResult;

            // Assert the JSON data content
            var jsonData = jsonResult.Value;
            var jObject = JObject.FromObject(jsonData);

            Assert.AreEqual(false, jObject["success"].Value<bool>());
            Assert.AreEqual("Error while deleting", jObject["message"].Value<string>());

            _unitOfWorkMock.Verify(repo => repo.Company.Remove(It.IsAny<Company>()), Times.Never);
            _unitOfWorkMock.Verify(repo => repo.Save(), Times.Never);
        }
    }
    }

