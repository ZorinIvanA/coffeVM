using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VirtualCoffee;
using VirtualCoffee.Repository;
using System.Xml;
using System.IO;

namespace VirtualCoffee.Tests.RepositoryTests
{
    [TestClass]
    public class DALTests
    {
        [TestInitialize]
        public void TestInitialize()
        {

        }

       

        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void TestLoadShouldThrowFileNotFound()
        {
            CoffeMachinePurseDataSet coffee = new CoffeMachinePurseDataSet();
            coffee.Load("bla-bla.xml");
        }

        [TestMethod]
        public void TestSaveShouldSuccess()
        {
            CoffeMachinePurseDataSet coffee = new CoffeMachinePurseDataSet();
            coffee.Item.Coins.Add(new Coin() { Count = 10, Value = "1" });
            coffee.Item.Coins.Add(new Coin() { Count = 10, Value = "2" });
            coffee.Save("mock.xml");

            XmlDocument xdoc = new XmlDocument();
            xdoc.Load("mock.xml");
            var coffeeM = xdoc.ChildNodes[0];

            var coins = coffeeM.ChildNodes[0];
            Assert.AreEqual(2, coins.ChildNodes.Count);
            Assert.AreEqual(10, Int32.Parse(coins.ChildNodes[0].Attributes["number"].Value));
            Assert.AreEqual(10, Int32.Parse(coins.ChildNodes[1].Attributes["number"].Value));
            Assert.AreEqual("1", coins.ChildNodes[0].Attributes["kind"].Value);
            Assert.AreEqual("2", coins.ChildNodes[1].Attributes["kind"].Value);
        }

        [TestMethod]
        public void TestCoffePurseInitializeShouldSuccess()
        {
            CoffeMachinePurseDataSet coffee = new CoffeMachinePurseDataSet();
            coffee.Initialize("coffee.xml");

            XmlDocument xdoc = new XmlDocument();
            xdoc.Load("coffee.xml");
            var coffeeM = xdoc.ChildNodes[0];
            var coins = coffeeM.ChildNodes[0];

            Assert.IsNotNull(coins);
            Assert.AreEqual(4, coins.ChildNodes.Count);
            Assert.AreEqual("1", coins.ChildNodes[0].Attributes["kind"].Value);
            Assert.AreEqual(100, Int32.Parse(coins.ChildNodes[0].Attributes["number"].Value));
            Assert.AreEqual("2", coins.ChildNodes[1].Attributes["kind"].Value);
            Assert.AreEqual(100, Int32.Parse(coins.ChildNodes[1].Attributes["number"].Value));
            Assert.AreEqual("5", coins.ChildNodes[2].Attributes["kind"].Value);
            Assert.AreEqual(100, Int32.Parse(coins.ChildNodes[2].Attributes["number"].Value));
            Assert.AreEqual("10", coins.ChildNodes[3].Attributes["kind"].Value);
            Assert.AreEqual(100, Int32.Parse(coins.ChildNodes[3].Attributes["number"].Value));

        }

        [TestMethod]
        public void TestCoffeGoodsInitializeShouldSuccess()
        {
            GoodsDataSet coffee = new GoodsDataSet();
            coffee.Initialize("goods.xml");

            XmlDocument xdoc = new XmlDocument();
            xdoc.Load("goods.xml");
            var coffeeM = xdoc.ChildNodes[0];
            var goods = coffeeM.ChildNodes[0];

            Assert.IsNotNull(goods);
            Assert.AreEqual(4, goods.ChildNodes.Count);
            Assert.AreEqual("tea", goods.ChildNodes[0].Attributes["kind"].Value);
            Assert.AreEqual(10, Int32.Parse(goods.ChildNodes[0].Attributes["number"].Value));
            Assert.AreEqual(13, Double.Parse(goods.ChildNodes[0].Attributes["price"].Value));
            Assert.AreEqual("coffee", goods.ChildNodes[1].Attributes["kind"].Value);
            Assert.AreEqual(20, Int32.Parse(goods.ChildNodes[1].Attributes["number"].Value));
            Assert.AreEqual(18, Double.Parse(goods.ChildNodes[1].Attributes["price"].Value));
            Assert.AreEqual("cappuccino", goods.ChildNodes[2].Attributes["kind"].Value);
            Assert.AreEqual(10, Int32.Parse(goods.ChildNodes[2].Attributes["number"].Value));
            Assert.AreEqual(21, Double.Parse(goods.ChildNodes[2].Attributes["price"].Value));
            Assert.AreEqual("juice", goods.ChildNodes[3].Attributes["kind"].Value);
            Assert.AreEqual(15, Int32.Parse(goods.ChildNodes[3].Attributes["number"].Value));
            Assert.AreEqual(35, Double.Parse(goods.ChildNodes[3].Attributes["price"].Value));
        }

        [TestMethod]
        public void UserPurseInitializeShouldSuccess()
        {
            UserPurseDataSet users = new UserPurseDataSet();
            users.Initialize("user.xml");

            XmlDocument xdoc = new XmlDocument();
            xdoc.Load("user.xml");
            var userPurse = xdoc.ChildNodes[0].ChildNodes[0];

            Assert.IsNotNull(userPurse);
            Assert.IsNotNull(userPurse.ChildNodes);
            Assert.AreEqual(4, userPurse.ChildNodes.Count);
            Assert.AreEqual("1", userPurse.ChildNodes[0].Attributes["kind"].Value);
            Assert.AreEqual(10, Int32.Parse(userPurse.ChildNodes[0].Attributes["number"].Value));
            Assert.AreEqual("2", userPurse.ChildNodes[1].Attributes["kind"].Value);
            Assert.AreEqual(30, Int32.Parse(userPurse.ChildNodes[1].Attributes["number"].Value));
            Assert.AreEqual("5", userPurse.ChildNodes[2].Attributes["kind"].Value);
            Assert.AreEqual(20, Int32.Parse(userPurse.ChildNodes[2].Attributes["number"].Value));
            Assert.AreEqual("10", userPurse.ChildNodes[3].Attributes["kind"].Value);
            Assert.AreEqual(15, Int32.Parse(userPurse.ChildNodes[3].Attributes["number"].Value));
        }

        [TestMethod]
        public void TestLoadShouldSucceed()
        {
            CoffeMachinePurseDataSet coffee = new CoffeMachinePurseDataSet();
            coffee.Initialize("coffee.xml"); //Так себе решение...
            coffee.Load("Coffee.xml");
            Assert.IsNotNull(coffee.Item);
            Assert.IsNotNull(coffee.Item.Coins);
            Assert.AreEqual(4, coffee.Item.Coins.Count);
            Assert.AreEqual("1", coffee.Item.Coins[0].Value);
            Assert.AreEqual(100, coffee.Item.Coins[0].Count);
            Assert.AreEqual("2", coffee.Item.Coins[1].Value);
            Assert.AreEqual(100, coffee.Item.Coins[1].Count);
            Assert.AreEqual("5", coffee.Item.Coins[2].Value);
            Assert.AreEqual(100, coffee.Item.Coins[2].Count);
            Assert.AreEqual("10", coffee.Item.Coins[3].Value);
            Assert.AreEqual(100, coffee.Item.Coins[3].Count);
        }

        [TestMethod]
        public void TestInitPurchase()
        {
            CoffeDataContext _ctx = new CoffeDataContext();
            _ctx.Init(String.Empty);
        }
    }
}
