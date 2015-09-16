using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VirtualCoffee.Repository;

namespace VirtualCoffee.Tests.Controllers
{
    public class PurseForTests : PurseBase
    {

    }

    [TestClass]
    public class PurseTests
    {
        [TestMethod]
        public void TestPurseSumShouldReturn27()
        {
            PurseForTests purse = new PurseForTests();
            purse.Coins.Add(new Coin() { Value = "10", Count = 1 });
            purse.Coins.Add(new Coin() { Value = "5", Count = 2 });
            purse.Coins.Add(new Coin() { Value = "2", Count = 3 });
            purse.Coins.Add(new Coin() { Value = "1", Count = 1 });

            Assert.AreEqual(27, purse.Sum);
        }
    }
}
