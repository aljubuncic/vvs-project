using Burgija.Controllers;
using Burgija.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System;
using System.Diagnostics.Eventing.Reader;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Burgija.Tests {
    [TestClass]
    public class TDDTests {
        private RentController rentController;
        private HomeController homeController;
        private List<ToolType> tools;
        private List<Discount> discounts;

        [TestInitialize]
        public void TestSetup() {
            rentController = new RentController(null);
            homeController = new HomeController(null, null);
            tools = new List<ToolType> {
                new ToolType(1, "Name", Category.GeneralConstruction,"", 0, ""),
                new ToolType(2, "Name", Category.GeneralConstruction,"", 0, ""),
                new ToolType(3, "Name", Category.GeneralConstruction,"", 0, ""),
                new ToolType(4, "Name", Category.GeneralConstruction,"", 0, ""),
                new ToolType(5, "Name", Category.GeneralConstruction,"", 0, ""),
                new ToolType(6, "Name", Category.GeneralConstruction,"", 0, ""),
                new ToolType(7, "Name", Category.GeneralConstruction,"", 0, ""),
                new ToolType(8, "Name", Category.GeneralConstruction,"", 0, ""),
                new ToolType(9, "Name", Category.GeneralConstruction,"", 0, ""),
                new ToolType(10, "Name", Category.GeneralConstruction,"", 0, ""),
                new ToolType(11, "Name", Category.FloorCare,"", 0, ""),
                new ToolType(12, "Name", Category.FloorCare,"", 0, ""),
                new ToolType(13, "Name", Category.FloorCare,"", 0, ""),
                new ToolType(14, "Name", Category.FloorCare,"", 0, ""),
                new ToolType(15, "Name", Category.FloorCare,"", 0, ""),
                new ToolType(16, "Name", Category.FloorCare,"", 0, ""),
                new ToolType(17, "Name", Category.FloorCare,"", 0, ""),
                new ToolType(18, "Name", Category.FloorCare,"", 0, ""),
                new ToolType(19, "Name", Category.FloorCare,"", 0, ""),
                new ToolType(20, "Name", Category.FloorCare,"", 0, ""),
        };
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
            Tool tool = new Tool(1, new ToolType(1, "Name", Category.GeneralConstruction, "Description", 10, ""), new Store(1, new Location()));
            int code = 1;

            double discountPrice = rentController.CalculateDiscount(tool, code, discounts);
            Assert.AreEqual(5, discountPrice);
        }

        [TestMethod]
        public void Discount_ValidCode2_ReturnsDiscountPrice() {
            Tool tool = new Tool(1, new ToolType(1, "Name", Category.GeneralConstruction, "Description", 10, ""), new Store(1, new Location()));
            int code = 2;

            double discountPrice = rentController.CalculateDiscount(tool, code, discounts);
            Assert.AreEqual(7.5, discountPrice);
        }

        [TestMethod]
        public void Discount_ValidCode3_ReturnsDiscountPrice() {
            Tool tool = new Tool(1, new ToolType(1, "Name", Category.GeneralConstruction, "Description", 10, ""), new Store(1, new Location()));
            int code = 3;

            double discountPrice = rentController.CalculateDiscount(tool, code, discounts);
            Assert.AreEqual(9, discountPrice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Discount_InvalidCode_ThrowsException() {
            Tool tool = new Tool(1, new ToolType(1, "Name", Category.GeneralConstruction, "Description", 10, ""), new Store(1, new Location()));
            int code = 10;

            double discountPrice = rentController.CalculateDiscount(tool, code, discounts);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Discount_ExpiredDate1_ThrowsException() {
            Tool tool = new Tool(1, new ToolType(1, "Name", Category.GeneralConstruction, "Description", 10, ""), new Store(1, new Location()));
            int code = 4;

            double discountPrice = rentController.CalculateDiscount(tool, code, discounts);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Discount_ExpiredDate2_ThrowsException() {
            Tool tool = new Tool(1, new ToolType(1, "Name", Category.GeneralConstruction, "Description", 10, ""), new Store(1, new Location()));
            int code = 5;

            double discountPrice = rentController.CalculateDiscount(tool, code, discounts);
        }

        [TestMethod]
        public void Suggestions_Valid1_ReturnsSuggestions() {
            List<ToolType> suggestions = new List<ToolType>();
            List<ToolType> history = new List<ToolType>() {
                new ToolType(1, "Name", Category.GeneralConstruction,"", 0, ""),
            };

            suggestions = homeController.SuggestedTools(history, tools);

            int count = 0;
            for (int i = 0; i < suggestions.Count; i++) {
                if (suggestions[i].Category == Category.GeneralConstruction)
                    count++;
            }

            Assert.AreEqual(4, count);
        }

        [TestMethod]
        public void Suggestions_Valid2_ReturnsSuggestions() {
            List<ToolType> suggestions = new List<ToolType>();
            List<ToolType> history = new List<ToolType>() {
                new ToolType(1, "Name", Category.GeneralConstruction,"", 0, ""),
                new ToolType(2, "Name", Category.GeneralConstruction,"", 0, ""),
                new ToolType(3, "Name", Category.GeneralConstruction,"", 0, ""),
                new ToolType(4, "Name", Category.GeneralConstruction,"", 0, ""),
            };

            suggestions = homeController.SuggestedTools(history, tools);

            int count = 0;
            for (int i = 0; i < suggestions.Count; i++) {
                if (suggestions[i].Category == Category.GeneralConstruction)
                    count++;
            }

            Assert.AreEqual(4, count);
        }

        [TestMethod]
        public void Suggestions_Valid3_ReturnsSuggestions() {
            List<ToolType> suggestions = new List<ToolType>();
            List<ToolType> history = new List<ToolType>() {
                new ToolType(1, "Name", Category.GeneralConstruction,"", 0, ""),
                new ToolType(11, "Name", Category.FloorCare,"", 0, ""),
            };

            suggestions = homeController.SuggestedTools(history, tools);

            int count = 0;
            for (int i = 0; i < suggestions.Count; i++) {
                if (suggestions[i].Category == Category.GeneralConstruction)
                    count++;
            }

            Assert.AreEqual(2, count);
        }

        [TestMethod]
        public void Suggestions_Valid4_ReturnsSuggestions() {
            List<ToolType> suggestions = new List<ToolType>();
            List<ToolType> history = new List<ToolType>() {
                new ToolType(1, "Name", Category.GeneralConstruction,"", 0, ""),
                new ToolType(2, "Name", Category.GeneralConstruction,"", 0, ""),
                new ToolType(11, "Name", Category.FloorCare,"", 0, ""),
                new ToolType(12, "Name", Category.FloorCare,"", 0, ""),
            };

            suggestions = homeController.SuggestedTools(history, tools);

            int count = 0;
            for (int i = 0; i < suggestions.Count; i++) {
                if (suggestions[i].Category == Category.GeneralConstruction)
                    count++;
            }

            Assert.AreEqual(2, count);
        }

        [TestMethod]
        public void Suggestions_Valid5_ReturnsSuggestions() {
            List<ToolType> suggestions = new List<ToolType>();
            List<ToolType> history = new List<ToolType>() {
                new ToolType(1, "Name", Category.GeneralConstruction,"", 0, ""),
                new ToolType(2, "Name", Category.GeneralConstruction,"", 0, ""),
                new ToolType(3, "Name", Category.GeneralConstruction,"", 0, ""),
                new ToolType(4, "Name", Category.GeneralConstruction,"", 0, ""),
                new ToolType(11, "Name", Category.FloorCare,"", 0, ""),
                new ToolType(12, "Name", Category.FloorCare,"", 0, ""),
                new ToolType(13, "Name", Category.FloorCare,"", 0, ""),
                new ToolType(14, "Name", Category.FloorCare,"", 0, ""),
            };

            suggestions = homeController.SuggestedTools(history, tools);

            int count = 0;
            for (int i = 0; i < suggestions.Count; i++) {
                if (suggestions[i].Category == Category.GeneralConstruction)
                    count++;
            }

            Assert.AreEqual(2, count);
        }

        [TestMethod]
        public void Suggestions_Valid6_ReturnsSuggestions() {
            List<ToolType> suggestions = new List<ToolType>();
            List<ToolType> history = new List<ToolType>() {
                new ToolType(1, "Name", Category.GeneralConstruction,"", 0, ""),
                new ToolType(11, "Name", Category.FloorCare,"", 0, ""),
                new ToolType(12, "Name", Category.FloorCare,"", 0, ""),
                new ToolType(13, "Name", Category.FloorCare,"", 0, ""),
            };

            suggestions = homeController.SuggestedTools(history, tools);

            int count = 0;
            for (int i = 0; i < suggestions.Count; i++) {
                if (suggestions[i].Category == Category.GeneralConstruction)
                    count++;
            }

            Assert.AreEqual(1, count);
        }

        [TestMethod]
        public void Suggestions_Valid7_ReturnsSuggestions() {
            List<ToolType> suggestions = new List<ToolType>();
            List<ToolType> history = new List<ToolType>() {
                new ToolType(1, "Name", Category.GeneralConstruction,"", 0, ""),
                new ToolType(2, "Name", Category.GeneralConstruction,"", 0, ""),
                new ToolType(11, "Name", Category.FloorCare,"", 0, ""),
                new ToolType(12, "Name", Category.FloorCare,"", 0, ""),
                new ToolType(13, "Name", Category.FloorCare,"", 0, ""),
                new ToolType(14, "Name", Category.FloorCare,"", 0, ""),
                new ToolType(15, "Name", Category.FloorCare,"", 0, ""),
                new ToolType(16, "Name", Category.FloorCare,"", 0, ""),
            };

            suggestions = homeController.SuggestedTools(history, tools);

            int count = 0;
            for (int i = 0; i < suggestions.Count; i++) {
                if (suggestions[i].Category == Category.GeneralConstruction)
                    count++;
            }

            Assert.AreEqual(1, count);
        }

        [TestMethod]
        public void Suggestions_Valid8_ReturnsSuggestions() {
            List<ToolType> suggestions = new List<ToolType>();
            List<ToolType> history = new List<ToolType>() {
                new ToolType(11, "Name", Category.FloorCare,"", 0, ""),
            };

            suggestions = homeController.SuggestedTools(history, tools);

            int count = 0;
            for (int i = 0; i < suggestions.Count; i++) {
                if (suggestions[i].Category == Category.GeneralConstruction)
                    count++;
            }

            Assert.AreEqual(0, count);
        }

        [TestMethod]
        public void Suggestions_Valid9_ReturnsSuggestions() {
            List<ToolType> suggestions = new List<ToolType>();
            List<ToolType> history = new List<ToolType>() {
                new ToolType(1, "Name", Category.GeneralConstruction,"", 0, ""),
                new ToolType(2, "Name", Category.GeneralConstruction,"", 0, ""),
                new ToolType(3, "Name", Category.GeneralConstruction,"", 0, ""),
                new ToolType(11, "Name", Category.FloorCare,"", 0, ""),
                new ToolType(12, "Name", Category.FloorCare,"", 0, ""),
            };

            suggestions = homeController.SuggestedTools(history, tools);

            int count = 0;
            for (int i = 0; i < suggestions.Count; i++) {
                if (suggestions[i].Category == Category.GeneralConstruction)
                    count++;
            }

            Assert.AreEqual(2, count);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Suggestions_EmptyHistory_ThrowsException() {
            List<ToolType> suggestions = new List<ToolType>();
            List<ToolType> history = new List<ToolType>();

            suggestions = homeController.SuggestedTools(history, tools);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Suggestions_InsufficientOffer_ThrowsException() {
            List<ToolType> suggestions = new List<ToolType>();
            List<ToolType> history = tools;

            suggestions = homeController.SuggestedTools(history, tools);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Suggestions_InsufficientOffer2_ThrowsException() {
            List<ToolType> suggestions = new List<ToolType>();
            List<ToolType> history = new List<ToolType>() {
                new ToolType(1, "Name", Category.GeneralConstruction,"", 0, ""),
                new ToolType(2, "Name", Category.GeneralConstruction,"", 0, ""),
                new ToolType(3, "Name", Category.GeneralConstruction,"", 0, ""),
                new ToolType(4, "Name", Category.GeneralConstruction,"", 0, ""),
                new ToolType(5, "Name", Category.GeneralConstruction,"", 0, ""),
                new ToolType(6, "Name", Category.GeneralConstruction,"", 0, ""),
                new ToolType(7, "Name", Category.GeneralConstruction,"", 0, ""),
            };

            suggestions = homeController.SuggestedTools(history, tools);
        }
    }
}
