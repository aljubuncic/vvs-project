using Burgija.Controllers;
using Burgija.Models;
using Burgija.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System;

namespace Burgija.Tests {
    [TestClass]
    public class TDDTests {
        private HomeController controller;
        private static List<Discount> discounts = new List<Discount>{
                new Discount(1, new DateTime(2023, 12, 1), new DateTime(2023, 12, 31), 50),
                new Discount(2, new DateTime(2023, 12, 1), new DateTime(2023, 12, 31), 25),
                new Discount(3, new DateTime(2023, 12, 1), new DateTime(2023, 12, 31), 10),
                new Discount(4, new DateTime(2023, 12, 1), new DateTime(2023, 12, 15), 50),
                new Discount(5, new DateTime(2024, 1, 1), new DateTime(2024, 1, 15), 50),
            };

        [TestInitialize]
        public void TestSetup() {
            controller = new HomeController(null, null);
        }

        [TestMethod]
        public void Discount_ValidCode1_ReturnsDiscountPrice() {
            Tool tool = new Tool(1, new ToolType(1, "Name", Category.GeneralConstruction, "Description", 10, ""), new Store(1, new Location()));
            int code = 1;

            double discountPrice = controller.CalculateDiscount(tool, code, discounts);
            Assert.AreEqual(5, discountPrice);
        }

        [TestMethod]
        public void Discount_ValidCode2_ReturnsDiscountPrice() {
            Tool tool = new Tool(1, new ToolType(1, "Name", Category.GeneralConstruction, "Description", 10, ""), new Store(1, new Location()));
            int code = 2;

            double discountPrice = controller.CalculateDiscount(tool, code, discounts);
            Assert.AreEqual(7.5, discountPrice);
        }

        [TestMethod]
        public void Discount_ValidCode3_ReturnsDiscountPrice() {
            Tool tool = new Tool(1, new ToolType(1, "Name", Category.GeneralConstruction, "Description", 10, ""), new Store(1, new Location()));
            int code = 3;

            double discountPrice = controller.CalculateDiscount(tool, code, discounts);
            Assert.AreEqual(9, discountPrice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Discount_InvalidCode_ThrowsException() {
            Tool tool = new Tool(1, new ToolType(1, "Name", Category.GeneralConstruction, "Description", 10, ""), new Store(1, new Location()));
            int code = 10;

            double discountPrice = controller.CalculateDiscount(tool, code, discounts);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Discount_ExpiredDate1_ThrowsException() {
            Tool tool = new Tool(1, new ToolType(1, "Name", Category.GeneralConstruction, "Description", 10, ""), new Store(1, new Location()));
            int code = 4;

            double discountPrice = controller.CalculateDiscount(tool, code, discounts);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Discount_ExpiredDate2_ThrowsException() {
            Tool tool = new Tool(1, new ToolType(1, "Name", Category.GeneralConstruction, "Description", 10, ""), new Store(1, new Location()));
            int code = 5;

            double discountPrice = controller.CalculateDiscount(tool, code, discounts);
        }
    }
}
