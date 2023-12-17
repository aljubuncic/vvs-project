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
using Burgija.ViewModels;
using System.Security.Claims;

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
            // Arrange
            var toolTypesList = ConvertToToolTypes(toolTypes);

            // For Users
            var user1 = new IdentityUser<int> { Id = 1 };
            var user2 = new IdentityUser<int> { Id = 2 };

            // For Locations
            var location1 = new Location(1, 40.7128, -74.0060, "New York City");
            var location2 = new Location(2, 34.0522, -118.2437, "Los Angeles");

            // For Stores
            var store1 = new Store(1, location1);
            var store2 = new Store(2, location2);

            // For Tools
            var tool1 = new Tool(1, toolTypesList[0], store1);
            var tool2 = new Tool(2, toolTypesList[1], store2);

            var rent1 = new Rent(1, user1, 1, tool1, 1, DateTime.Now, DateTime.Now.AddDays(10), null, null, 25.00);
            var rent2 = new Rent(2, user1, 1, tool2, 2, DateTime.Now, DateTime.Now.AddDays(10), null, null, 27.00);

            // Create lists to hold the objects
            var users = new List<IdentityUser<int>> { user1, user2 };
            var locations = new List<Location> { location1, location2 };
            var stores = new List<Store> { store1, store2 };
            var tools = new List<Tool> { tool1, tool2 };

            // Arrange

            var mockSet2 = MockDbSet.Create(users);
            var mockSet3 = MockDbSet.Create(tools);
            var mockSet4 = MockDbSet.Create(stores);
            var mockSet5 = MockDbSet.Create(locations);
            var mockSet6 = MockDbSet.Create(toolTypesList);

            var mockContext = new Mock<IApplicationDbContext>();
            mockContext.Setup(c => c.Users).Returns(mockSet2.Object);
            mockContext.Setup(c => c.Tools).Returns(mockSet3.Object);
            mockContext.Setup(c => c.Stores).Returns(mockSet4.Object);
            mockContext.Setup(c => c.Locations).Returns(mockSet5.Object);

            // Mocking the User context
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, "123"), // Sample user ID
        new Claim(ClaimTypes.Role, "RegisteredUser"), // Simulate being in the "RegisteredUser" role
    };

            // Create a ClaimsIdentity with the claims
            var identity = new ClaimsIdentity(claims, "TestAuth");

            // Create a ClaimsPrincipal with the ClaimsIdentity
            var user = new ClaimsPrincipal(identity);

            // Mocking UserManager<IdentityUser<int>>
            var mockUserStore = new Mock<IUserStore<IdentityUser<int>>>();
            var mockUserManager = new Mock<UserManager<IdentityUser<int>>>(mockUserStore.Object, null, null, null, null, null, null, null, null);
            var mockRentSet = MockDbSet.Create(new List<Rent> { rent1, rent2 });
            mockContext.Setup(c => c.Rent).Returns(mockRentSet.Object);

            var mockToolSet = MockDbSet.Create(new List<Tool> { tool1, tool2 });
            mockContext.Setup(c => c.Tool).Returns(mockToolSet.Object);

            var mockToolTypeSet = MockDbSet.Create(toolTypesList);
            mockContext.Setup(c => c.ToolType).Returns(mockToolTypeSet.Object);

            var controller = new RentController(mockContext.Object);

            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

            // Setup FindByIdAsync to return a mock IdentityUser<int>
            mockUserManager
                .Setup(u => u.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((string id) => new IdentityUser<int> { UserName = "MockUserName" });

            // Act
            var result = await controller.RentHistory() as ViewResult;

            // Assert
            Assert.IsNotNull(result);

            // Check if the model is of the correct type
            var model = result.Model as List<RentAndToolType>;
            Assert.IsNotNull(model);

            Assert.IsInstanceOfType(result, typeof(ViewResult));

            Assert.IsInstanceOfType(result.Model, typeof(List<RentAndToolType>));

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
            dbContextMock.Setup(mock => mock.ToolTypes).Returns(mockSet.Object);
            // Set up the mock for FirstOrDefaultAsync
            //mockSet.Setup(mock => mock.FirstOrDefaultAsync(tt => tt.Id == It.IsAny<int>())).ReturnsAsync((ToolType)null);

            var rentController = new RentController(dbContextMock.Object);
            //Act
            var result = await rentController.Create(toolTypeId);
            //Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Create_ToolTypeIdIsInDb_ReturnsViewResult()
        {
            //Arrange
            // Create a mock DbSet using the MockDbSet class
            var mockSet = MockDbSet.Create(ConvertToToolTypes(toolTypes));
            int toolTypeId = 1;
            // Setup mock DbContext
            dbContextMock.Setup(mock => mock.ToolTypes).Returns(mockSet.Object);
            // Set up the mock for FirstOrDefaultAsync
            //mockSet.Setup(mock => mock.FirstOrDefaultAsync(tt => tt.Id == It.IsAny<int>())).ReturnsAsync((ToolType)null);

            var rentController = new RentController(dbContextMock.Object);
            //Act
            var result = await rentController.Create(toolTypeId);
            //Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
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
