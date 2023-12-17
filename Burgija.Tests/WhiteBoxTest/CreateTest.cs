using Burgija.Controllers;
using Burgija.Interfaces;
using Burgija.Models;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Burgija.Tests.WhiteBoxTest
{
    [TestClass]
    public class CreateTest
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
        public async Task Path1()
        {
            // Arrange

            // Act
            var result = await controller.Create(null);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
        [TestMethod]
        public async Task Path2()
        {
            var mockSet = MockDbSet.Create(ConvertToToolTypes(toolTypes));
            int toolTypeId = -1;

            // Setup mock DbContext
            dbContextMock.Setup(mock => mock.ToolTypes).Returns(mockSet.Object);

            
            //Act
            var result = await controller.Create(toolTypeId);
            //Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Path3()
        {
            //Arrange
            // Create a mock DbSet using the MockDbSet class
            var mockSet = MockDbSet.Create(ConvertToToolTypes(toolTypes));
            int toolTypeId = 1;
            // Setup mock DbContext
            dbContextMock.Setup(mock => mock.ToolTypes).Returns(mockSet.Object);

            //Act
            var result = await controller.Create(toolTypeId);
            //Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        static IEnumerable<object[]> toolTypes
        {
            get
            {
                return UcitajPodatkeCSV("tools.csv");
            }
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
