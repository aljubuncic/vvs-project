using Burgija.Controllers;
using Burgija.Interfaces;
using Burgija.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Burgija.Tests.WhiteBoxTest
{
    /// <summary>
    /// Class for white box testing of method HomeController.LinearSearch
    /// </summary>
    [TestClass]
    public class LinearSearchTest
    {
      
        [TestMethod]
        public void LinearSearch_SearchIsNull_ReturnsEmptyList()
        {
            //Act
            var result = HomeController.LinearSearch(null, null);
            //Assert
            CollectionAssert.AreEquivalent(new List<ToolType>(), result);
        }

        [TestMethod]
        public void LinearSearch_SearchIsWhiteSpace_ReturnsEmptyList()
        {
            //Act
            var result = HomeController.LinearSearch(null, "  ");
            //Assert
            CollectionAssert.AreEquivalent(new List<ToolType>(), result);
        }

        [TestMethod]
        public void LinearSearch_ListIsEmpty_ReturnsEmptyList()
        {
            //Act
            var result = HomeController.LinearSearch(new List<ToolType>(), "");
            //Assert
            CollectionAssert.AreEquivalent(new List<ToolType>(), result);
        }

        [TestMethod]
        [DynamicData(nameof(toolTypesJson))]
        public void LinearSearch_OneElementInListWithMatchingSearch_ReturnsList(List<ToolType> toolTypes)
        {
            //Arrange
            var toolTypesList = toolTypes.GetRange(0, 1);
            //Act
            var result = HomeController.LinearSearch(toolTypesList, "hammer");
            //Assert
            var expected = new List<ToolType>
            {
                new ToolType(1,"Hammer",Category.GeneralConstruction,"Tool for hammering nails",12,"")
            };
            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        [DynamicData(nameof(toolTypesJson))]
        public void LinearSearch_OneElementInListWithNoMatchingSearch_ReturnsEmptyList(List<ToolType> toolTypes)
        {
            //Arrange
            var toolTypesList = toolTypes.GetRange(0, 1);
            //Act
            var result = HomeController.LinearSearch(toolTypesList, "no");
            //Assert
            var expected = new List<ToolType>();
            
            CollectionAssert.AreEqual(expected, result);
        }

        private static IEnumerable<object[]> toolTypesJson
        {
            get
            {
                string jsonFilePath = "../../../WhiteBoxTest/toolTypes.json";
                string jsonData = File.ReadAllText(jsonFilePath);
                var testData = JsonConvert.DeserializeObject<List<ToolType>>(jsonData);
                yield return new object[] { testData };

            }
        }
    }
}
