using Burgija.Controllers;
using Burgija.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Burgija.Tests.WhiteBoxTest
{


    [TestClass]
    public class LinearSearchByPriceTest
    {
        private static List<ToolType> tools = new List<ToolType>
{
    new ToolType
    {
        Id = 1,
        Name = "Hammer",
        Category = Category.GeneralConstruction,
        Description = "Tool for hammering nails",
        Price = 2.99,
        Image = "hammer.jpg"
    },
    new ToolType
    {
        Id = 2,
        Name = "Screwdriver",
        Category = Category.Fasteners,
        Description = "Tool for driving screws",
        Price = 8.49,
        Image = "screwdriver.jpg"
    }

};

        [TestMethod]
        public void Path1()
        {
            HomeController.LinearSearchByPrice(tools, 5, 10);
        }

        [TestMethod]
        public void Path2()
        {
            HomeController.LinearSearchByPrice(tools, 55, 100);
        }
    }
}
