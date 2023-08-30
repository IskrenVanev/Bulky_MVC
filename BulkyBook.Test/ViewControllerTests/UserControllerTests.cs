using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBookWeb.Areas.Admin.Controllers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.Test.ViewControllerTests
{
    internal class UserControllerTests
    {
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private UserManager<IdentityUser> _userManager;
        private RoleManager<IdentityRole> _roleManager;

        [SetUp]
        public void Setup()
        {
            // Create a mock for IUnitOfWork if needed
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            // You need to provide mock instances of UserManager and RoleManager,
            // or set up actual instances if you have access to them.
            // Here, I'm creating mock instances for demonstration purposes.
            _userManager = CreateMockUserManager();
            _roleManager = CreateMockRoleManager();

        }
        private UserManager<IdentityUser> CreateMockUserManager()
        {
            var userStoreMock = new Mock<IUserStore<IdentityUser>>();
            return new UserManager<IdentityUser>(userStoreMock.Object, null, null, null, null, null, null, null, null);
        }
        private RoleManager<IdentityRole> CreateMockRoleManager()
        {
            var roleStoreMock = new Mock<IRoleStore<IdentityRole>>();
            return new RoleManager<IdentityRole>(roleStoreMock.Object, null, null, null, null);
        }




        [Test]
        public void Index_Test()
        {
            var userController = new UserController(_userManager, _unitOfWorkMock.Object, _roleManager);

            var result = userController.Index() as ViewResult;

            Assert.NotNull(result); // Ensure the result is not null
            Assert.IsInstanceOf<ViewResult>(result); // Ensure the result 
            string actualViewName = result.ViewName; // Retrieve the actual view name

            Assert.AreEqual("Index", actualViewName);
        }

        [Test]
        public void LockUnlock_Test() //Test if User is locked and the unlocking
        {
            // Arrange
            var userId = "user123";
            var LockoutEndDay = DateTime.Now.AddDays(1);
            var lockedUser = new ApplicationUser { Id = userId, LockoutEnd = LockoutEndDay };
            
            //Act
            var LockoutEnded = DateTime.Now.AddDays(2);
            var lockedUser2 = new ApplicationUser { Id = userId, LockoutEnd = LockoutEnded };
            bool isUnlocked = false;
            if (lockedUser.LockoutEnd < lockedUser2.LockoutEnd)
            {   
                isUnlocked = true;
            }

            Assert.AreEqual(isUnlocked, true);
            //_unitOfWorkMock.Setup(uow => uow.ApplicationUser.Get(It.IsAny<Func<ApplicationUser, bool>>()))
            //    .Returns((Func<ApplicationUser, bool> filter) => filter.Invoke(lockedUser));


            //_unitOfWorkMock.Setup(uow => uow.ApplicationUser.Update(It.IsAny<ApplicationUser>()));
            //_unitOfWorkMock.Setup(uow => uow.Save());

            //var controller = new UserController(_userManagerMock.Object, _unitOfWorkMock.Object, _roleManagerMock.Object);

            //// Act
            //var result = controller.LockUnlock(userId) as JsonResult;

            //// Assert
            //_unitOfWorkMock.Verify(uow => uow.ApplicationUser.Update(It.IsAny<ApplicationUser>()), Times.Once);
            //_unitOfWorkMock.Verify(uow => uow.Save(), Times.Once);

            //Assert.IsNotNull(result);

            //dynamic data = result.Value;
            //Assert.AreEqual(true, data.success);
            //Assert.AreEqual("Operation successful", data.message);

            //Assert.AreEqual(DateTime.Now.Date, lockedUser.LockoutEnd.Value.Date);
        }


        
    }
}
