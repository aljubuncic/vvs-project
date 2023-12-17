using Burgija.Controllers;
using Burgija.Models;

using System.Globalization;
using Microsoft.AspNetCore.Identity;

using Burgija.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;
using System.Linq;
using System.IO;
using Burgija.Interfaces;
using System.Threading.Tasks;
using System;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using CsvHelper;
using System.Xml.Serialization;
using System.Xml;

namespace Burgija.Tests {
    public class TestDbContext : DbContext {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

        public DbSet<ToolType> ToolType { get; set; } // Add DbSet for ToolType (assuming ToolType is your model entity)
                                                      // Other DbSet properties as needed for testing other entities
    }

    [TestClass]
    public class HomeControllerTests {
        private HomeController controller;
        private Mock<IApplicationDbContext> mockDbContext;
        private ApplicationDbContext mockContext;


        [TestInitialize]
        public void SetUp() {
            controller = new HomeController(null, null);
            mockDbContext = new Mock<IApplicationDbContext>();
        }

        public static IEnumerable<object[]> toolTypesJson {
            get {
                string jsonFilePath = "tools.json";
                string jsonData = File.ReadAllText(jsonFilePath);
                var testData = JsonConvert.DeserializeObject<List<List<ToolType>>>(jsonData);

                foreach (var list in testData) {
                    yield return new object[] { list };
                }
            }
        }

        [TestMethod]
        [DynamicData(nameof(toolTypesJson))]
        public void QuickSort(List<ToolType> tools) {
            // Arrange
            bool sorted = true;
            // Act
            List<ToolType> sortedTools = tools;
            HomeController.QuickSort(sortedTools);
            // Assert
            for (int i = 1; i < sortedTools.Count; i++)
                if (sortedTools[i].Price < sortedTools[i - 1].Price)
                    sorted = false;
            Assert.AreEqual(true, sorted);
        }

        [TestMethod]
        [DynamicData(nameof(toolTypesJson))]
        public void SelectionSort(List<ToolType> tools) {
            // Arrange
            bool sorted = true;
            // Act
            List<ToolType> sortedTools = tools;
            HomeController.SelectionSortDescending(sortedTools);
            // Assert
            for (int i = 1; i < sortedTools.Count; i++)
                if (sortedTools[i].Price > sortedTools[i - 1].Price)
                    sorted = false;
            Assert.AreEqual(true, sorted);
        }
        public static IEnumerable<object[]> toolTypesXml {
            get {
                XmlDocument doc = new XmlDocument();
                doc.Load("tools.xml");
                foreach (XmlNode xmlList in doc.DocumentElement.ChildNodes) {
                    List<ToolType> tools = new List<ToolType>();
                    foreach (XmlNode xmlTool in xmlList.ChildNodes) {
                        ToolType tool = new ToolType();
                        tool.Id = int.Parse(xmlTool.SelectSingleNode("Id").InnerText);
                        tool.Name = xmlTool.SelectSingleNode("Name").InnerText;
                        tool.Category = (Category)int.Parse(xmlTool.SelectSingleNode("Category").InnerText);
                        tool.Price = int.Parse(xmlTool.SelectSingleNode("Price").InnerText);
                        tool.Description = xmlTool.SelectSingleNode("Description").InnerText;
                        tool.Image = xmlTool.SelectSingleNode("Image").InnerText;
                        tools.Add(tool);
                    }
                    yield return new object[] { tools };
                }
            }
        }


        [TestMethod]
        [DynamicData(nameof(toolTypesXml))]
        public void MergeSort(List<ToolType> tools) {
            // Arrange
            bool sorted = true;
            // Act
            List<ToolType> sortedTools = HomeController.MergeSort(tools);
            // Assert
            for (int i = 1; i < sortedTools.Count; i++)
                if (sortedTools[i].Name.CompareTo(sortedTools[i - 1].Name) < 0)
                    sorted = false;
            Assert.AreEqual(true, sorted);
        }


        static IEnumerable<object[]> toolTypes {
            get {
                return UcitajPodatkeCSV("tools.csv");
            }
        }


        [TestMethod]
        public async Task Test_Index_returnAllAlati() {

            // Create a mock DbSet using the MockDbSet class
            var mockSet = MockDbSet.Create(ConvertToToolTypes(toolTypes));

            // Setup mock DbContext
            var mockDbContext = new Mock<IApplicationDbContext>();
            mockDbContext.Setup(c => c.ToolTypes).Returns(mockSet.Object);

            var mockUserManager = new Mock<UserManager<IdentityUser<int>>>(
             new Mock<IUserStore<IdentityUser<int>>>().Object,
              null, null, null, null, null, null, null, null);

            controller = new HomeController(mockDbContext.Object, mockUserManager.Object);

            // Act
            var result = await controller.Index(null, null, null, null) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            // Assert.IsType<List<ToolType>>(result.Model);
            Assert.AreEqual(4, (result.Model as List<ToolType>).Count);
        }
        [TestMethod]
        public async Task Test_Index_returnSearch() {

            // Create a mock DbSet using the MockDbSet class
            var mockSet = MockDbSet.Create(ConvertToToolTypes(toolTypes));

            // Setup mock DbContext
            var mockDbContext = new Mock<IApplicationDbContext>();
            mockDbContext.Setup(c => c.ToolTypes).Returns(mockSet.Object);

            // Create a mock mockUserManager using the Mock class
            var mockUserManager = new Mock<UserManager<IdentityUser<int>>>(
             new Mock<IUserStore<IdentityUser<int>>>().Object,
              null, null, null, null, null, null, null, null);

            controller = new HomeController(mockDbContext.Object, mockUserManager.Object);

            // Act
            var result = await controller.Index("Hammer", 5, 15, null) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            // Jedan je alat Hammer u listi
            Assert.AreEqual(1, (result.Model as List<ToolType>).Count);

            var check = result.Model as List<ToolType>;

            Assert.IsTrue(check[0].Price >= 5 && check[0].Price <= 15);
        }
        [TestMethod]
        public async Task Test_Index_returnPriceFilteredAlatiByLowestPrice() {

            // Create a mock DbSet using the MockDbSet class
            var mockSet = MockDbSet.Create(ConvertToToolTypes(toolTypes));

            // Setup mock DbContext
            var mockDbContext = new Mock<IApplicationDbContext>();
            mockDbContext.Setup(c => c.ToolTypes).Returns(mockSet.Object);

            // Create a mock mockUserManager using the Mock class
            var mockUserManager = new Mock<UserManager<IdentityUser<int>>>(
             new Mock<IUserStore<IdentityUser<int>>>().Object,
              null, null, null, null, null, null, null, null);

            controller = new HomeController(mockDbContext.Object, mockUserManager.Object);

            // Act
            var result = await controller.Index(null, 5, 10, "lowestPrice") as ViewResult;

            // Assert
            Assert.IsNotNull(result);

            // Dva alata imaju cijenu između 5-10
            Assert.AreEqual(2, (result.Model as List<ToolType>).Count);

            var check = result.Model as List<ToolType>;

            check.Sort((tool1, tool2) => tool2.Price.CompareTo(tool1.Price));

            CollectionAssert.AreEqual(check, result.Model as List<ToolType>);
        }

        [TestMethod]
        public async Task Test_Index_returnPriceFilteredAlatiByHighestPrice() {

            // Create a mock DbSet using the MockDbSet class
            var mockSet = MockDbSet.Create(ConvertToToolTypes(toolTypes));

            // Setup mock DbContext
            var mockDbContext = new Mock<IApplicationDbContext>();
            mockDbContext.Setup(c => c.ToolTypes).Returns(mockSet.Object);

            // Create a mock mockUserManager using the Mock class
            var mockUserManager = new Mock<UserManager<IdentityUser<int>>>(
             new Mock<IUserStore<IdentityUser<int>>>().Object,
              null, null, null, null, null, null, null, null);

            controller = new HomeController(mockDbContext.Object, mockUserManager.Object);

            // Act
            var result = await controller.Index(null, 12, 50, "highestPrice") as ViewResult;

            // Assert
            Assert.IsNotNull(result);

            // Dva alata imaju cijenu između 15-50
            Assert.AreEqual(2, (result.Model as List<ToolType>).Count);

            var check = result.Model as List<ToolType>;

            check.Sort((tool1, tool2) => tool1.Price.CompareTo(tool2.Price));

            CollectionAssert.AreEqual(check, result.Model as List<ToolType>);
        }

        [TestMethod]
        public async Task Test_Index_returnPriceFilteredAlatiByName() {

            // Create a mock DbSet using the MockDbSet class
            var mockSet = MockDbSet.Create(ConvertToToolTypes(toolTypes));

            // Setup mock DbContext
            var mockDbContext = new Mock<IApplicationDbContext>();
            mockDbContext.Setup(c => c.ToolTypes).Returns(mockSet.Object);

            // Create a mock mockUserManager using the Mock class
            var mockUserManager = new Mock<UserManager<IdentityUser<int>>>(
             new Mock<IUserStore<IdentityUser<int>>>().Object,
              null, null, null, null, null, null, null, null);

            controller = new HomeController(mockDbContext.Object, mockUserManager.Object);

            // Act
            var result = await controller.Index(null, 8, 15, "alphabetical") as ViewResult;

            // Assert
            Assert.IsNotNull(result);

            // Dva alata imaju cijenu između 8-15
            Assert.AreEqual(2, (result.Model as List<ToolType>).Count);

            var check = result.Model as List<ToolType>;

            check.Sort((tool1, tool2) => tool2.Name.CompareTo(tool1.Name));

            CollectionAssert.AreEqual(check, result.Model as List<ToolType>);
        }

        [TestMethod]
        public async Task Test_Method_returnFilteredTools() {

            // Create a mock DbSet using the MockDbSet class
            var mockSet = MockDbSet.Create(ConvertToToolTypes(toolTypes));

            // Setup mock DbContext
            var mockDbContext = new Mock<IApplicationDbContext>();
            mockDbContext.Setup(c => c.ToolTypes).Returns(mockSet.Object);

            // Create a mock mockUserManager using the Mock class
            var mockUserManager = new Mock<UserManager<IdentityUser<int>>>(
             new Mock<IUserStore<IdentityUser<int>>>().Object,
              null, null, null, null, null, null, null, null);

            controller = new HomeController(mockDbContext.Object, mockUserManager.Object);

            // Act
            var result = await controller.FilterTools(5, 55, "lowestPrice");

            // Assert(<RedirectToActionResult>(result));
            Assert.IsTrue(result is RedirectToActionResult, "Expected a RedirectToActionResult");
            var redirectToActionResult = (RedirectToActionResult)result;

            Assert.AreEqual("Index", redirectToActionResult.ActionName);
            Assert.IsNotNull(redirectToActionResult.RouteValues);

            // Verify the query parameters
            Assert.IsTrue(redirectToActionResult.RouteValues.TryGetValue("priceFrom", out var priceFromValue));
            Assert.AreEqual(5.ToString(), priceFromValue);

            Assert.IsTrue(redirectToActionResult.RouteValues.TryGetValue("priceTo", out var priceToValue));
            Assert.AreEqual(55.ToString(), priceToValue);

            Assert.IsTrue(redirectToActionResult.RouteValues.TryGetValue("sortOptions", out var sortOptionsValue));
            Assert.AreEqual("lowestPrice", sortOptionsValue);
        }



        [TestMethod]
        public async Task WhereYouCanFindUs_ReturnsViewWithStoresAndLocations() {
            var stores = new List<Store>
        {
            new Store(1, new Location(1, 40.7128, -74.0060, "New York City")),
            new Store(2, new Location(2, 34.0522, -118.2437, "Los Angeles"))
            // Add more stores as needed
        };

            var locations = new List<Location>
        {
            new Location(1, 40.7128, -74.0060, "New York City"),
            new Location(2, 34.0522, -118.2437, "Los Angeles")
            // Add more locations as needed
        };

            var mockSet = MockDbSet.Create(stores);
            var mockSet2 = MockDbSet.Create(locations);

            var mockDbContext = new Mock<IApplicationDbContext>();
            mockDbContext.Setup(c => c.Stores).Returns(mockSet.Object);
            mockDbContext.Setup(c => c.Locations).Returns(mockSet2.Object);

            var mockUserManager = new Mock<UserManager<IdentityUser<int>>>(
                 new Mock<IUserStore<IdentityUser<int>>>().Object,
                 null, null, null, null, null, null, null, null);


            var controller = new HomeController(mockDbContext.Object, mockUserManager.Object);


            // Act
            var result = await controller.WhereYouCanFindUs() as ViewResult;
            var checkStores = result.ViewData.Values.ElementAt(0) as List<Store>;
            var checkLocations = result.ViewData.Values.ElementAt(1) as List<Location>;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("FindUs", result.ViewName);

            // Check the count of stores and locations
            Assert.AreEqual(stores.Count, checkStores.Count);
            Assert.AreEqual(locations.Count, checkLocations.Count);

            // Compare individual elements or properties
            for (var i = 0; i < stores.Count; i++) {
                Assert.AreEqual(stores[i].Id, checkStores[i].Id);
            }

            for (var i = 0; i < locations.Count; i++) {
                Assert.AreEqual(locations[i].Address, checkLocations[i].Address);
            }
        }

        [TestMethod]
        public async Task ToolDetails_ReturnsCorrectView() {
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

            // For Reviews
            var review1 = new Review(1, user1, tool1, rent1, DateTimeOffset.UtcNow.ToUnixTimeSeconds(), "Great tool!", 4.5);
            var review2 = new Review(2, user2, tool2, rent2, DateTimeOffset.UtcNow.ToUnixTimeSeconds(), "Good product.", 4.0);

            // Create lists to hold the objects
            var users = new List<IdentityUser<int>> { user1, user2 };
            var locations = new List<Location> { location1, location2 };
            var stores = new List<Store> { store1, store2 };
            var tools = new List<Tool> { tool1, tool2 };
            var reviews = new List<Review> { review1, review2 };

            // Arrange
            var toolId = 1; // Replace with the desired tool ID for testing

            var mockSet = MockDbSet.Create(reviews);
            var mockSet2 = MockDbSet.Create(users);
            var mockSet3 = MockDbSet.Create(tools);
            var mockSet4 = MockDbSet.Create(stores);
            var mockSet5 = MockDbSet.Create(locations);
            var mockSet6 = MockDbSet.Create(toolTypesList);

            var mockContext = new Mock<IApplicationDbContext>();
            mockContext.Setup(c => c.ToolTypes).Returns(mockSet6.Object);
            mockContext.Setup(c => c.Reviews).Returns(mockSet.Object);
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

            var controller = new HomeController(mockContext.Object, mockUserManager.Object);

            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

            // Setup FindByIdAsync to return a mock IdentityUser<int>
            mockUserManager
                .Setup(u => u.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((string id) => new IdentityUser<int> { UserName = "MockUserName" });

            // Act
            var result = await controller.ToolDetails(toolId) as ViewResult;

            // Assert
            Assert.IsNotNull(result);

            var checkTool = result.Model as ToolType;
            Assert.AreEqual(checkTool.Id, toolId);

            // id je null metoda vraća null
            result = await controller.ToolDetails(null) as ViewResult;
            Assert.IsNull(result);

            // nepostojeći id, metoda vraća null
            var result2 = await controller.ToolDetails(1000) as ViewResult;
            Assert.IsNull(result2);

        }

        [TestMethod]
        public async Task Test_SendReviews()
        {
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

            var rent1 = new Rent(1, user1, 1, tool1, 1, DateTime.Now.AddDays(-10), DateTime.Now.AddDays(10), null, null, 25.00);
            var rent2 = new Rent(2, user1, 1, tool2, 2, DateTime.Now.AddMonths(-1), DateTime.Now.AddDays(10), null, null, 27.00);

            // For Reviews
           var review1 = new Review(1, user1, tool1, rent1, DateTimeOffset.UtcNow.ToUnixTimeSeconds(), "Great tool!", 4.5);
           var review2 = new Review(2, user2, tool2, rent2, DateTimeOffset.UtcNow.ToUnixTimeSeconds(), "Good product.", 4.0);

            // Create lists to hold the objects
            var users = new List<IdentityUser<int>> { user1, user2 };
            var locations = new List<Location> { location1, location2 };
            var stores = new List<Store> { store1, store2 };
            var tools = new List<Tool> { tool1, tool2 };
            var reviews = new List<Review> { review1, review2 };
            var rents = new List<Rent> { rent1, rent2 };

            var mockSet = MockDbSet.Create(reviews);
            var mockSet2 = MockDbSet.Create(users);
            var mockSet3 = MockDbSet.Create(tools);
            var mockSet4 = MockDbSet.Create(stores);
            var mockSet5 = MockDbSet.Create(locations);
            var mockSet6 = MockDbSet.Create(toolTypesList);
            var mockSet7 = MockDbSet.Create(rents);

            var mockContext = new Mock<IApplicationDbContext>();
            mockContext.Setup(c => c.ToolTypes).Returns(mockSet6.Object);
            mockContext.Setup(c => c.Reviews).Returns(mockSet.Object);
            mockContext.Setup(c => c.Users).Returns(mockSet2.Object);
            mockContext.Setup(c => c.Tools).Returns(mockSet3.Object);
            mockContext.Setup(c => c.Stores).Returns(mockSet4.Object);
            mockContext.Setup(c => c.Locations).Returns(mockSet5.Object);
            mockContext.Setup(c => c.Rents).Returns(mockSet7.Object);

            // Mocking the User context
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1"), // Sample user ID
                new Claim(ClaimTypes.Role, "RegisteredUser"), // Simulate being in the "RegisteredUser" role
  
            };

            // Create a ClaimsIdentity with the claims
            var identity = new ClaimsIdentity(claims, "TestAuth");

            // Create a ClaimsPrincipal with the ClaimsIdentity
            var user = new ClaimsPrincipal(identity);

            // Mocking UserManager<IdentityUser<int>>
            var mockUserStore = new Mock<IUserStore<IdentityUser<int>>>();
            var mockUserManager = new Mock<UserManager<IdentityUser<int>>>(mockUserStore.Object, null, null, null, null, null, null, null, null);

            var controller = new HomeController(mockContext.Object, mockUserManager.Object);

            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

            // Setup FindByIdAsync to return a mock IdentityUser<int>
            mockUserManager
                .Setup(u => u.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((string id) => new IdentityUser<int> { UserName = "MockUserName" });

            // Act
            var result = await controller.SendReview(review1, 1, "Great tool!", 4.5) as RedirectToActionResult;
            Assert.AreEqual(result.ActionName, "ToolDetails");
            var check = result.RouteValues.Values;
            Assert.AreEqual(check.First(),1);

            var result2 = await controller.SendReview(review2, 13, "Good product.", 4.0) as BadRequestObjectResult;
            Assert.AreEqual(result2.Value, "You have not rented this tool before!");

        }

        public static IEnumerable<object[]> UcitajPodatkeCSV(string path) {
            using (var reader = new StreamReader(path))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture)) {
                var rows = csv.GetRecords<dynamic>();
                foreach (var row in rows) {
                    var values = ((IDictionary<String, Object>)row).Values;
                    var elements = values.Select(elem => elem.ToString()).ToList();
                    yield return new object[] { elements[0], elements[1],elements[2],
                        elements[3],elements[4],elements[5] };

                }

            }
        }

        public List<ToolType> ConvertToToolTypes(IEnumerable<object[]> toolTypeData) {
            List<ToolType> toolTypes = new List<ToolType>();

            foreach (var row in toolTypeData) {
                // Assuming the columns in the IEnumerable<object[]> match the properties of ToolType
                var toolType = new ToolType {
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