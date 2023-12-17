using Burgija.Controllers;
using Burgija.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Burgija.Tests {
    [TestClass]
    public class TDDTests {
        private RentController rentController;
        private HomeController homeController;
        private List<ToolType> tools;
        private List<Discount> discounts;

        public List<ToolType> CreateToolsByCategory(int cat1, int cat2) {
            List<ToolType> tools = new List<ToolType>();
            for (int i = 1; i <= cat1; i++)
                tools.Add(new ToolType(i, "name", Category.GeneralConstruction, "", 0, ""));
            for (int i = 1; i <= cat2; i++)
                tools.Add(new ToolType(10 + i, "name", Category.FloorCare, "", 0, ""));
            return tools;
        }

        [TestInitialize]
        public void TestSetup() {
            rentController = new RentController(null);
            homeController = new HomeController(null, null);
            tools = CreateToolsByCategory(10, 10);
            discounts = new List<Discount>{
                new Discount(1, new DateTime(2023, 12, 1), new DateTime(2023, 12, 31), 50),
                new Discount(2, new DateTime(2023, 12, 1), new DateTime(2023, 12, 31), 25),
                new Discount(3, new DateTime(2023, 12, 1), new DateTime(2023, 12, 31), 10),
                new Discount(4, new DateTime(2023, 12, 1), new DateTime(2023, 12, 15), 50),
                new Discount(5, new DateTime(2024, 1, 1), new DateTime(2024, 1, 15), 50),
            };
        }

        [TestMethod]
        public void Discount_ValidCode1_ReturnsDiscountPrice() {
            // Arrange
            Tool tool = new Tool(1, new ToolType(1, "Name", Category.GeneralConstruction, "Description", 10, ""), new Store(1, new Location()));
            int code = 1;
            // Act
            double discountPrice = rentController.CalculateDiscount(tool, code, discounts);
            // Assert
            Assert.AreEqual(5, discountPrice);
        }

        [TestMethod]
        public void Discount_ValidCode2_ReturnsDiscountPrice() {
            // Arrange
            Tool tool = new Tool(1, new ToolType(1, "Name", Category.GeneralConstruction, "Description", 10, ""), new Store(1, new Location()));
            int code = 2;
            // Act
            double discountPrice = rentController.CalculateDiscount(tool, code, discounts);
            // Assert
            Assert.AreEqual(7.5, discountPrice);
        }

        [TestMethod]
        public void Discount_ValidCode3_ReturnsDiscountPrice() {
            // Arrange
            Tool tool = new Tool(1, new ToolType(1, "Name", Category.GeneralConstruction, "Description", 10, ""), new Store(1, new Location()));
            int code = 3;
            // Act
            double discountPrice = rentController.CalculateDiscount(tool, code, discounts);
            // Assert
            Assert.AreEqual(9, discountPrice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Discount_InvalidCode_ThrowsException() {
            // Arrange
            Tool tool = new Tool(1, new ToolType(1, "Name", Category.GeneralConstruction, "Description", 10, ""), new Store(1, new Location()));
            int code = 10;
            // Act
            double discountPrice = rentController.CalculateDiscount(tool, code, discounts);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Discount_ExpiredDate1_ThrowsException() {
            // Arrange
            Tool tool = new Tool(1, new ToolType(1, "Name", Category.GeneralConstruction, "Description", 10, ""), new Store(1, new Location()));
            int code = 4;
            // Act
            double discountPrice = rentController.CalculateDiscount(tool, code, discounts);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Discount_ExpiredDate2_ThrowsException() {
            // Arrange
            Tool tool = new Tool(1, new ToolType(1, "Name", Category.GeneralConstruction, "Description", 10, ""), new Store(1, new Location()));
            int code = 5;
            // Act
            double discountPrice = rentController.CalculateDiscount(tool, code, discounts);
        }

        [TestMethod]
        public void Suggestions_ValidHistory1_ReturnsSuggestions() {
            // Arrange
            List<ToolType> suggestions = new List<ToolType>();
            List<ToolType> history = CreateToolsByCategory(1, 0);
            // Act
            suggestions = homeController.SuggestedTools(history, tools);
            // Assert
            int count = suggestions.Count(x => x.Category == Category.GeneralConstruction);
            bool hasDuplicates = suggestions.GroupBy(obj => obj.Id).Any(group => group.Count() > 1);
            Assert.AreEqual(false, hasDuplicates);
            Assert.AreEqual(4, count);
        }

        [TestMethod]
        public void Suggestions_ValidHistory2_ReturnsSuggestions() {
            // Arrange
            List<ToolType> suggestions = new List<ToolType>();
            List<ToolType> history = CreateToolsByCategory(4, 0);
            // Act
            suggestions = homeController.SuggestedTools(history, tools);
            // Assert
            int count = suggestions.Count(x => x.Category == Category.GeneralConstruction);
            bool hasDuplicates = suggestions.GroupBy(obj => obj.Id).Any(group => group.Count() > 1);
            Assert.AreEqual(false, hasDuplicates);
            Assert.AreEqual(4, count);
        }

        [TestMethod]
        public void Suggestions_ValidHistory3_ReturnsSuggestions() {
            // Arrange
            List<ToolType> suggestions = new List<ToolType>();
            List<ToolType> history = CreateToolsByCategory(1, 1);
            // Act
            suggestions = homeController.SuggestedTools(history, tools);
            // Assert
            int count = suggestions.Count(x => x.Category == Category.GeneralConstruction);
            bool hasDuplicates = suggestions.GroupBy(obj => obj.Id).Any(group => group.Count() > 1);
            Assert.AreEqual(false, hasDuplicates);
            Assert.AreEqual(2, count);
        }

        [TestMethod]
        public void Suggestions_ValidHistory4_ReturnsSuggestions() {
            // Arrange
            List<ToolType> suggestions = new List<ToolType>();
            List<ToolType> history = CreateToolsByCategory(2, 2);
            // Act
            suggestions = homeController.SuggestedTools(history, tools);
            // Assert
            int count = suggestions.Count(x => x.Category == Category.GeneralConstruction);
            bool hasDuplicates = suggestions.GroupBy(obj => obj.Id).Any(group => group.Count() > 1);
            Assert.AreEqual(false, hasDuplicates);
            Assert.AreEqual(2, count);
        }

        [TestMethod]
        public void Suggestions_ValidHistory5_ReturnsSuggestions() {
            // Arrange
            List<ToolType> suggestions = new List<ToolType>();
            List<ToolType> history = CreateToolsByCategory(4, 4);
            // Act
            suggestions = homeController.SuggestedTools(history, tools);
            // Assert
            int count = suggestions.Count(x => x.Category == Category.GeneralConstruction);
            bool hasDuplicates = suggestions.GroupBy(obj => obj.Id).Any(group => group.Count() > 1);
            Assert.AreEqual(false, hasDuplicates);
            Assert.AreEqual(2, count);
        }

        [TestMethod]
        public void Suggestions_ValidHistory6_ReturnsSuggestions() {
            // Arrange
            List<ToolType> suggestions = new List<ToolType>();
            List<ToolType> history = CreateToolsByCategory(1, 3);
            // Act
            suggestions = homeController.SuggestedTools(history, tools);
            // Assert
            int count = suggestions.Count(x => x.Category == Category.GeneralConstruction);
            bool hasDuplicates = suggestions.GroupBy(obj => obj.Id).Any(group => group.Count() > 1);
            Assert.AreEqual(false, hasDuplicates);
            Assert.AreEqual(1, count);
        }

        [TestMethod]
        public void Suggestions_ValidHistory7_ReturnsSuggestions() {
            // Arrange
            List<ToolType> suggestions = new List<ToolType>();
            List<ToolType> history = CreateToolsByCategory(2, 6);
            // Act
            suggestions = homeController.SuggestedTools(history, tools);
            // Assert
            int count = suggestions.Count(x => x.Category == Category.GeneralConstruction);
            bool hasDuplicates = suggestions.GroupBy(obj => obj.Id).Any(group => group.Count() > 1);
            Assert.AreEqual(false, hasDuplicates);
            Assert.AreEqual(1, count);
        }

        [TestMethod]
        public void Suggestions_ValidHistory8_ReturnsSuggestions() {
            // Arrange
            List<ToolType> suggestions = new List<ToolType>();
            List<ToolType> history = CreateToolsByCategory(0, 1);
            // Act
            suggestions = homeController.SuggestedTools(history, tools);
            // Assert
            int count = suggestions.Count(x => x.Category == Category.GeneralConstruction);
            bool hasDuplicates = suggestions.GroupBy(obj => obj.Id).Any(group => group.Count() > 1);
            Assert.AreEqual(false, hasDuplicates);
            Assert.AreEqual(0, count);
        }

        [TestMethod]
        public void Suggestions_ValidHistory9_ReturnsSuggestions() {
            // Arrange
            List<ToolType> suggestions = new List<ToolType>();
            List<ToolType> history = CreateToolsByCategory(3, 2);
            // Act
            suggestions = homeController.SuggestedTools(history, tools);
            // Assert
            int count = suggestions.Count(x => x.Category == Category.GeneralConstruction);
            bool hasDuplicates = suggestions.GroupBy(obj => obj.Id).Any(group => group.Count() > 1);
            Assert.AreEqual(false, hasDuplicates);
            Assert.AreEqual(2, count);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Suggestions_EmptyHistory_ThrowsException() {
            // Arrange
            List<ToolType> suggestions = new List<ToolType>();
            List<ToolType> history = new List<ToolType>();
            // Act
            suggestions = homeController.SuggestedTools(history, tools);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Suggestions_InsufficientOffer1_ThrowsException() {
            // Arrange
            List<ToolType> suggestions = new List<ToolType>();
            List<ToolType> history = tools;
            // Act
            suggestions = homeController.SuggestedTools(history, tools);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Suggestions_InsufficientOffer2_ThrowsException() {
            // Arrange
            List<ToolType> suggestions = new List<ToolType>();
            List<ToolType> history = CreateToolsByCategory(7, 0);
            // Act
            suggestions = homeController.SuggestedTools(history, tools);
        }
    }
}
