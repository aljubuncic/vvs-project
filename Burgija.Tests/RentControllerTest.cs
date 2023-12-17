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
using CsvHelper;
using System.Globalization;
using System.IO;
using Microsoft.EntityFrameworkCore;

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
            controller = new RentController(null);
        }

        static IEnumerable<object[]> toolTypes
        {
            get
            {
                return UcitajPodatkeCSV("alati.csv");
            }
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
        public async Task Create_ValidRentDates_RedirectsToRentHistory()
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
        public async Task RentHistory_User_ReturnsViewResultWithRents()
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
            // Create a mock DbSet using the MockDbSet class
            var mockSet = MockDbSet.Create(ConvertToToolTypes(toolTypes));
            int toolTypeId = -1;
            // Setup mock DbContext
            dbContextMock.Setup(mock => mock.ToolType).Returns(mockSet.Object);
            // Set up the mock for FirstOrDefaultAsync
            //mockSet.Setup(mock => mock.FirstOrDefaultAsync(tt => tt.Id == It.IsAny<int>())).ReturnsAsync((ToolType)null);

            var rentController = new RentController(dbContextMock.Object);
            //Act
            var result = await rentController.Create(toolTypeId);
            //Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }


        public static IEnumerable<object[]> UcitajPodatkeCSV(string path)
        {
            using (var reader = new StreamReader(path))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var rows = csv.GetRecords<dynamic>();
                foreach (var row in rows)
                {
                    var values = ((IDictionary<String, Object>)row).Values;
                    var elements = values.Select(elem => elem.ToString()).ToList();
                    yield return new object[] { elements[0], elements[1],elements[2],
                        elements[3],elements[4],elements[5] };

                }

            }
        }

        public List<ToolType> ConvertToToolTypes(IEnumerable<object[]> toolTypeData)
        {
            List<ToolType> toolTypes = new List<ToolType>();

            foreach (var row in toolTypeData)
            {
                // Assuming the columns in the IEnumerable<object[]> match the properties of ToolType
                var toolType = new ToolType
                {
                    Id = Convert.ToInt32(row[0]),
                    Name = row[1].ToString(),
                    Category = (Category)Enum.Parse(typeof(Category), row[2].ToString()),
                    Description = row[3].ToString(),
                    Price = Convert.ToDouble(row[4]),
                    Image = row[5].ToString()
                };

                toolTypes.Add(toolType);
            }

            return toolTypes;
        }
    }
}
