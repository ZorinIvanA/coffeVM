using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VirtualCoffee.Controllers;
using VirtualCoffee.Repository;
using System.Collections.Generic;

namespace VirtualCoffee.Tests.Controllers
{
    public class GoodsRepositoryMock : GoodsRepository
    {
        public GoodsRepositoryMock()
            : base()
        {

        }
    }

    public class CoffeRepositoryMock : CoffeeMachineRepository
    {
        public CoffeRepositoryMock()
            : base()
        {
            this.Coins = new Dictionary<String, Int32>();
            this.Coins.Add("1", 100);
            this.Coins.Add("2", 100);
            this.Coins.Add("5", 100);
            this.Coins.Add("10", 100);
        }
    }

    public class UserPurseRepositoryMock : UserPurseRepository
    {
        public UserPurseRepositoryMock()
            : base()
        {
            this.Coins = new Dictionary<String, Int32>();
            this.Coins.Add("1", 10);
            this.Coins.Add("2", 30);
            this.Coins.Add("5", 20);
            this.Coins.Add("10", 15);
        }
    }

    public class CoffeMachineControllerForTests : CoffeeMachineController
    {
        public GoodsRepository Goods { get { return _goods; } }
        public UserPurseRepository UserPurse { get { return _userPurse; } }
        public CoffeeMachineRepository Coffee { get { return _coffee; } }

        public CoffeMachineControllerForTests() : base() { }

        public CoffeMachineControllerForTests(GoodsRepository goods, UserPurseRepository userPurse,
            CoffeeMachineRepository coffeeMachine)
            : base() { }
    }

    [TestClass]
    public class CoffeeControllerTests
    {
        CoffeMachineControllerForTests _controller;

        [TestInitialize]
        public void InitializeTests()
        {
            GoodsRepositoryMock goods = new GoodsRepositoryMock();
            UserPurseRepositoryMock purse = new UserPurseRepositoryMock();
            CoffeRepositoryMock coffe = new CoffeRepositoryMock();
            _controller = new CoffeMachineControllerForTests(goods, purse, coffe);
        }


        [TestMethod]
        public void TestGetUserPurseShouldReturnInitialValues()
        {
            var purse = _controller.GetUserPurse().Result;
            Assert.IsNotNull(purse);
            Assert.IsNotNull(purse.Coins);
            Assert.AreEqual(4, purse.Coins.Length);
            Assert.AreEqual("1", purse.Coins[0].value);
            Assert.AreEqual("10", purse.Coins[0].count);
            Assert.AreEqual("2", purse.Coins[1].value);
            Assert.AreEqual("30", purse.Coins[1].count);
            Assert.AreEqual("5", purse.Coins[2].value);
            Assert.AreEqual("20", purse.Coins[2].count);
            Assert.AreEqual("10", purse.Coins[3].value);
            Assert.AreEqual("15", purse.Coins[3].count);
        }

        [TestMethod]
        public void TestGetMachinePurse()
        {
            var purse = _controller.GetMachinePurse().Result;
            Assert.IsNotNull(purse);
            Assert.AreEqual(4, purse.Coins.Length);
            Assert.AreEqual("1", purse.Coins[0].value);
            Assert.AreEqual("100", purse.Coins[0].count);
            Assert.AreEqual("2", purse.Coins[1].value);
            Assert.AreEqual("100", purse.Coins[1].count);
            Assert.AreEqual("5", purse.Coins[2].value);
            Assert.AreEqual("100", purse.Coins[2].count);
            Assert.AreEqual("10", purse.Coins[3].value);
            Assert.AreEqual("100", purse.Coins[3].count);
        }

        [TestMethod]
        public void TestPayCoinShouldSucceed()
        {
            var result = _controller.PayCoin(1).Result;
            Assert.AreEqual(9, _controller.UserPurse.Coins["1"]);
            Assert.AreEqual(101, _controller.Coffee.Coins["1"]);

        }
    }
}
