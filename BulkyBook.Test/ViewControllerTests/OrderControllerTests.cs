using BulkyBook.DataAccess.Repository;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBookWeb.Areas.Admin.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using BulkyBook.Utility;

namespace BulkyBook.Test.ViewControllerTests
{
    [TestFixture]
    public class OrderControllerTests
    {

        private Mock<IUnitOfWork> _unitOfWorkMock;
        private Mock<IOrderHeaderRepository> _orderHeaderRepositoryMock;
        private Mock<IOrderDetailRepository> _orderDetailRepositoryMock;

        //private Fixture _fixture;
        private OrderController _orderController;

        [SetUp]
        public void Setup()
        {
            //_fixture = new Fixture();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _orderController = new OrderController(_unitOfWorkMock.Object);

        }

        [Test]
        public void Index_View_Test()
        {


            var result = _orderController.Index() as ViewResult;

            // Assert
            Assert.NotNull(result);

            Assert.IsInstanceOf<ViewResult>(result);
            

            Assert.AreEqual("Index", result.ViewName);

        }

        [Test]
        public void Details_Returns_ViewResult_With_OrderVm()
        {
            // Arrange
            int orderId = 1;
            
            var controller = new OrderController(_unitOfWorkMock.Object);

            var fakeOrderHeader = new OrderHeader { Id = orderId };
            var fakeOrderDetails = new List<OrderDetail>
            {
                new OrderDetail { Id = 1, OrderHeaderId = orderId },
                new OrderDetail { Id = 2, OrderHeaderId = orderId }
            };
            var fakeOrderVm = new OrderVM
            {
                OrderHeader = fakeOrderHeader,
                OrderDetail = fakeOrderDetails
            };

            _unitOfWorkMock.Setup(uow => uow.OrderHeader.Get(It.IsAny<Expression<Func<OrderHeader, bool>>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(fakeOrderHeader);
            _unitOfWorkMock.Setup(uow => uow.OrderDetail.GetAll(It.IsAny<Expression<Func<OrderDetail, bool>>>(), It.IsAny<string>()))
                .Returns(fakeOrderDetails);

            // Act
            var result = controller.Details(orderId);

            // Assert

            Assert.IsInstanceOf<ViewResult>(result);
            var viewResult = result as ViewResult;
            Assert.AreEqual("Details", viewResult.ViewName);
            Assert.IsInstanceOf<OrderVM>(viewResult.Model);
            var orderVm = viewResult.Model as OrderVM;
            Assert.AreEqual(fakeOrderVm.OrderHeader.Id, orderVm.OrderHeader.Id);
            Assert.AreEqual(fakeOrderVm.OrderDetail.Count(), orderVm.OrderDetail.Count());
        }

    //    [Test]
    //    public void UpdateOrderDetail_Returns_RedirectToAction()
    //    {
    //        // Arrange
    //        int orderId = 12;
    //        var fakeApplicationUser = new ApplicationUser
    //        {
    //            Id = "8956d084-c120-469f-b12d-39404e387890",
    //            Name = "John Doe",
    //            StreetAddress = "123 Main St",
    //            City = "Example City",
    //            State = "CA",
    //            PostalCode = "12345",
    //            CompanyId = 1,
    //            Role = SD.Role_Admin

       

    //    // Populate other properties as needed
    //};
    //        var fakeOrderHeader = new OrderHeader
    //        {
    //            Id = orderId,
    //            ApplicationUserId = "8956d084-c120-469f-b12d-39404e387890",
    //            ApplicationUser = fakeApplicationUser,
    //            OrderDate = DateTime.Now.AddDays(-7),
    //            ShippingDate = DateTime.Now.AddDays(-5),
    //            OrderTotal = 150.0,
    //            OrderStatus = "Shipped",
    //            PaymentStatus = "Approved",
    //            TrackingNumber = "123456789",
    //            Carrier = "FedEx",
    //            PaymentDate = DateTime.Now.AddDays(-7),
    //            PaymentDueDate = new DateOnly(2023, 12, 31),
    //            SessionId = "session123",
    //            PaymentIntentId = "intent123",
    //            PhoneNumber = "123-456-7890",
    //            StreetAddress = "123 Main St",
    //            City = "Example City",
    //            State = "CA",
    //            PostalCode = "12345",
    //            Name = "John Doe"
    //        }; 

    //        var fakeOrderDetails = new List<OrderDetail>
    //        {
    //            new OrderDetail { Id = 1, OrderHeaderId = orderId },
    //            new OrderDetail { Id = 2, OrderHeaderId = orderId+1 }
    //        };
    //        var fakeOrderVm = new OrderVM
    //        {
    //            OrderHeader = fakeOrderHeader,
    //            OrderDetail = fakeOrderDetails

    //            // Populate other properties as needed
    //        };
          
           
    //        // Mocking
    //        _unitOfWorkMock.Setup(repo => repo.OrderHeader.Get(
    //                It.IsAny<Expression<Func<OrderHeader, bool>>>(),
    //                It.IsAny<string>(),
    //                It.IsAny<bool>()
    //            ))
    //            .Returns((Expression<Func<OrderHeader, bool>> filter, string includeProperties, bool tracked) =>
    //            {
    //                if (filter.Compile().Invoke(fakeOrderHeader))
    //                {
    //                    return fakeOrderHeader;
    //                }

    //                // Return null or default if the filter doesn't match (handle as needed)
    //                return null; // You can return null or default here, depending on your test scenario

    //            });

    //        _unitOfWorkMock.Setup(db => db.OrderHeader.Update(It.IsAny<OrderHeader>()));
    //        _unitOfWorkMock.Setup(db => db.Save()).Verifiable();
            
            
    //        //Act
    //        var result = _orderController.UpdateOrderDetail();

    //        // Assert
    //        _unitOfWorkMock.Verify(db => db.OrderHeader.Update(It.IsAny<OrderHeader>()), Times.Once);
    //        _unitOfWorkMock.Verify(db => db.Save(), Times.Once);
    //        var redirectToActionResult = result as RedirectToActionResult;
    //        Assert.AreEqual("Details", redirectToActionResult.ActionName);
    //        Assert.AreEqual(fakeOrderVm.OrderHeader.Id, redirectToActionResult.RouteValues["orderId"]);

    //        // Additional assertions can be added to check TempData and other behavior
    //    }


    }


    }

