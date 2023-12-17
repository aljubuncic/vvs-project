using Burgija.Controllers;
using Burgija.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Burgija.Tests {
    [TestClass]
    public class CalculateDiscountTest {
        private RentController rentController;
        private List<Discount> discounts;

        [TestInitialize]
        public void TestSetup() {
            rentController = new RentController(null);
            discounts = new List<Discount>{
                new Discount(1, new DateTime(2023, 12, 1), new DateTime(2023, 12, 31), 50),
                new Discount(2, new DateTime(2023, 12, 1), new DateTime(2023, 12, 31), 25),
                new Discount(3, new DateTime(2023, 12, 1), new DateTime(2023, 12, 31), 10),
                new Discount(4, new DateTime(2023, 12, 1), new DateTime(2023, 12, 15), 50),
                new Discount(5, new DateTime(2024, 1, 1), new DateTime(2024, 1, 15), 50),
            };
        }

        [TestMethod]
        public void PathCoverage_Path3() {
            // Arrange
            Tool tool = new Tool(1, new ToolType(1, "Name", Category.GeneralConstruction, "Description", 10, ""), new Store(1, new Location()));
            int code = 1;
            // Act
            double discountPrice = rentController.CalculateDiscount(tool, code, discounts);
            // Assert
            Assert.AreEqual(5, discountPrice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void PathCoverage_Path2() {
            // Arrange
            Tool tool = new Tool(1, new ToolType(1, "Name", Category.GeneralConstruction, "Description", 10, ""), new Store(1, new Location()));
            int code = 10;
            // Act
            double discountPrice = rentController.CalculateDiscount(tool, code, discounts);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void PathCoverage_Path1() {
            // Arrange
            Tool tool = new Tool(1, new ToolType(1, "Name", Category.GeneralConstruction, "Description", 10, ""), new Store(1, new Location()));
            int code = 4;
            // Act
            double discountPrice = rentController.CalculateDiscount(tool, code, discounts);
        }
    }
}
