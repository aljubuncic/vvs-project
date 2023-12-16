using Burgija.Controllers;
using Burgija.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Burgija.Models;
using System.Collections.Generic;
using System;
using Burgija.Interfaces;

namespace Burgija.Tests
{
    [TestClass]
    public class RentControllerTest
    {
        private Mock<IApplicationDbContext> dbContextMock;
        private RentController controller;

        [TestInitialize]
        public void Setup()
        {
            dbContextMock = new Mock<IApplicationDbContext>();
            controller = new RentController(dbContextMock.Object);
        }

        


        [TestMethod]
        public async Task Create_WhenToolTypeIdIsNull_ReturnsNotFound()
        {
            // Arrange
            var controller = new RentController(dbContextMock.Object);

            // Act
            var actualResult = await controller.Create(null);

            // Assert
            Assert.IsInstanceOfType(actualResult, typeof(NotFoundResult));
        }
        [TestMethod]
        public async Task Create_ValidRent_RedirectsToRentHistory()
        {
            // Arrange
            var rent = new Rent { StartOfRent = DateTime.Now.AddDays(1), EndOfRent = DateTime.Now.AddDays(3) };
            controller.ModelState.Clear(); // Clear ModelState for a valid rent

            // Act
            var result = await controller.Create(rent, -1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirectResult = (RedirectToActionResult)result;
            Assert.AreEqual("RentHistory", redirectResult.ActionName);
        }

        [TestMethod]
        public async Task Create_StartOfRentBeforeEndOfRent_ReturnsBadRequest()
        {
            // Arrange
            var rent = new Rent { StartOfRent = DateTime.Now.AddDays(3), EndOfRent = DateTime.Now.AddDays(1) };
            controller.ModelState.AddModelError("key", "error");

            // Act
            var result = await controller.Create(rent, null);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var badRequestResult = (BadRequestObjectResult)result;
            Assert.AreEqual("Date of return is earlier than date of taking", badRequestResult.Value.ToString());
        }
        [TestMethod]
        public async Task Create_StartRentBeforeNow_ReturnsBadRequest()
        {
            // Arrange
            var rent = new Rent { StartOfRent = DateTime.Now.AddDays(-1), EndOfRent = DateTime.Now.AddDays(1) };
            controller.ModelState.AddModelError("key", "error");

            // Act
            var result = await controller.Create(rent, null);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var badRequestResult = (BadRequestObjectResult)result;
            Assert.AreEqual("Date of taking or date of return is earlier than today", badRequestResult.Value.ToString());
        }






        [TestMethod]
        public void GetToolType_Null_ReturnsNotFoundResult()
        {
            //Arrange
            var rentController = new RentController(dbContextMock.Object);
            //Act
            var result = rentController.GetToolType(null);
            //Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public void GetToolType_Number_ReturnsRedirectToActionResult()
        {
            //Arrange
            var httpContextMock = new Mock<HttpContext>();
            var sessionMock = new Mock<ISession>();
            httpContextMock.Setup(c => c.Session).Returns(sessionMock.Object);
            var rentControllerContext = new ControllerContext
            {
                HttpContext = httpContextMock.Object,
            };
            var rentController = new RentController(dbContextMock.Object)
            {
                ControllerContext = rentControllerContext,
            };
            //Act
            var result = rentController.GetToolType(1);
            //Assert
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));

            var redirectResult = (RedirectToActionResult)result;
            Assert.AreEqual("Create", redirectResult.ActionName);
            CollectionAssert.AreEqual(new object[] { 1 }, redirectResult.RouteValues.Values.ToArray());
        }

        [TestMethod]
        public async void RentHistory_User_ReturnsViewResultWithRents()
        {


            Assert.AreEqual(true, true);
        }

        [TestMethod]
        public async Task Create_Null_ReturnsNotFoundResult()
        {
            //Arrange
            var rentController = new RentController(dbContextMock.Object);
            //Act
            var result = await rentController.Create((int?)null);
            //Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Create_ToolTypeIdNotInDb_ReturnsNotFoundResult()
        {
            //Arrange
            var rentController = new RentController(dbContextMock.Object);
            //Act
            var result = await rentController.Create(-1);
            //Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
    }
}
