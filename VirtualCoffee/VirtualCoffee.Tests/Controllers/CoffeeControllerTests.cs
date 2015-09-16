﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Linq;

using VirtualCoffee.Controllers;
using VirtualCoffee.Repository;
using System.Collections.Generic;
using VirtualCoffee.Models;

namespace VirtualCoffee.Tests.Controllers
{
    public class DataContextMock : CoffeDataContext
    {
        public override void Load(String baseUrl)
        {
            Goods.Load("goods.xml");
            CoffeMachinePurse.Load("coffee.xml");
            UserPurse.Load("user.xml");
        }

        public override void Save(String baseUrl)
        {
            Goods.Save("goods.xml");
            CoffeMachinePurse.Save("coffee.xml");
            UserPurse.Save("user.xml");
        }

        public override void Init(String baseUrl)
        {
            Goods.Initialize("goods.xml");
            CoffeMachinePurse.Initialize("coffee.xml");
            UserPurse.Initialize("user.xml");
        }
    }


    public class CoffeMachineControllerForTests : CoffeeMachineController
    {
        public CoffeMachineControllerForTests(DataContextMock ctx) : base(ctx) { }

        public PurseBase MakeChangeForTests(Double sum)
        {
            return this.MakeChange(sum);
        }

        public void MakeUpdatesInPursesForTests(PurseBase coinsToChange)
        {
            this.MakeUpdatesInPurses(coinsToChange);
        }
    }

    [TestClass]
    public class CoffeeControllerTests
    {
        DataContextMock _ctx;
        CoffeMachineControllerForTests _controller;

        [TestInitialize]
        public void InitializeTests()
        {
            _ctx = new DataContextMock();
            _ctx.Init(String.Empty);
            _controller = new CoffeMachineControllerForTests(_ctx);

        }

        [TestMethod]
        public void TestGetUserPurseShouldSuccess()
        {
            PurseModel userPurse = _controller.GetUserPurse().Result;
            Assert.IsNotNull(userPurse);
            Assert.IsNotNull(userPurse.Coins);
            Assert.AreEqual(4, userPurse.Coins.Length);
            Assert.AreEqual("1", userPurse.Coins[0].value);
            Assert.AreEqual("10", userPurse.Coins[0].count);
            Assert.AreEqual("2", userPurse.Coins[1].value);
            Assert.AreEqual("30", userPurse.Coins[1].count);
            Assert.AreEqual("5", userPurse.Coins[2].value);
            Assert.AreEqual("20", userPurse.Coins[2].count);
            Assert.AreEqual("10", userPurse.Coins[3].value);
            Assert.AreEqual("15", userPurse.Coins[3].count);
        }

        [TestMethod]
        public void TestGetMachinePurseShouldSuccess()
        {
            PurseModel machinePurse = _controller.GetMachinePurse().Result;
            Assert.IsNotNull(machinePurse);
            Assert.IsNotNull(machinePurse.Coins);
            Assert.AreEqual(4, machinePurse.Coins.Length);
            Assert.AreEqual("1", machinePurse.Coins[0].value);
            Assert.AreEqual("100", machinePurse.Coins[0].count);
            Assert.AreEqual("2", machinePurse.Coins[1].value);
            Assert.AreEqual("100", machinePurse.Coins[1].count);
            Assert.AreEqual("5", machinePurse.Coins[2].value);
            Assert.AreEqual("100", machinePurse.Coins[2].count);
            Assert.AreEqual("10", machinePurse.Coins[3].value);
            Assert.AreEqual("100", machinePurse.Coins[3].count);
        }

        [TestMethod]
        public void TestPayCoinShouldSuccess()
        {
            _ctx.Init(String.Empty);
            var result = _controller.PayCoin(1).Result;
            Assert.AreEqual(String.Empty, result.GetType().GetProperty("error").GetValue(result));
            Assert.AreEqual(9, _ctx.UserPurse.Item.Coins[0].Count);
            Assert.AreEqual(101, _ctx.CoffeMachinePurse.Item.Coins[0].Count);
            Assert.AreEqual(1, _ctx.PurchaseInfo.Item.PayedSum);
        }

        [TestMethod]
        public void TestPayCoinShouldSendErrorWhenMoneyFinished()
        {
            _ctx.Init(String.Empty);
            for (Int32 i = 0; i < 10; i++)
            {
                var temp = _controller.PayCoin(1).Result;
            }
            var result = _controller.PayCoin(1).Result;
            Assert.IsNotNull(result.GetType().GetProperty("error"));
            Assert.AreEqual("Монеты номиналом 1 закончились!",
                result.GetType().GetProperty("error").GetValue(result));
        }

        [TestMethod]
        public void TestPayCoinShouldSendErrorWhenWrongCoin()
        {
            _ctx.Init(String.Empty);

            var result = _controller.PayCoin(20).Result;
            Assert.IsNotNull(result.GetType().GetProperty("error"));
            Assert.AreEqual("Монеты номиналом 20 не существует!",
                result.GetType().GetProperty("error").GetValue(result));
        }

        [TestMethod]
        public void TestBuyItemShouldSuccess()
        {
            _ctx.Init(String.Empty);
            _ctx.PurchaseInfo.Item.PayedSum = 13;
            var result = _controller.BuyItem("tea").Result;
            Assert.AreEqual(9, _ctx.Goods.Item.Goods[0].Count);
            Assert.AreEqual("Вы купили tea, спасибо за покупку!",
                result.GetType().GetProperty("message").GetValue(result));
            Assert.AreEqual(String.Empty, result.GetType().GetProperty("error").GetValue(result));
            Assert.AreEqual(0, _ctx.PurchaseInfo.Item.PayedSum);
        }

        [TestMethod]
        public void TestBuyItemShouldFailWhenLowFunds()
        {
            _ctx.Init(String.Empty);
            _ctx.PurchaseInfo.Item.PayedSum = 10;
            var result = _controller.BuyItem("tea").Result;
            Assert.AreEqual(String.Empty,
                result.GetType().GetProperty("message").GetValue(result));
            Assert.AreEqual("Вы внесли недостаточно средств для покупки tea!",
                result.GetType().GetProperty("error").GetValue(result));
        }

        [TestMethod]
        public void TestBuyItemShouldFailWhenLowGoods()
        {
            _ctx.Init(String.Empty);
            _ctx.PurchaseInfo.Item.PayedSum = 15;
            _ctx.Goods.Item.Goods[0].Count = 0;
            var result = _controller.BuyItem("tea").Result;
            Assert.AreEqual(String.Empty,
                result.GetType().GetProperty("message").GetValue(result));
            Assert.AreEqual("Товар tea закончился!",
                result.GetType().GetProperty("error").GetValue(result));
        }

        [TestMethod]
        public void TestBuyItemShouldFailWhenWrongGoods()
        {
            _ctx.Init(String.Empty);
            _ctx.PurchaseInfo.Item.PayedSum = 15;
            var result = _controller.BuyItem("sugar").Result;
            Assert.AreEqual(String.Empty,
                result.GetType().GetProperty("message").GetValue(result));
            Assert.AreEqual("Товара sugar нет в автомате!",
                result.GetType().GetProperty("error").GetValue(result));
        }

        [TestMethod]
        public void TestGetChangeShouldSucceed()
        {
            _ctx.Init(String.Empty);
            _ctx.PurchaseInfo.Item.PayedSum = 15;
            var result = _controller.GetChange().Result;
            Assert.AreEqual(16, _ctx.UserPurse.Item.Coins[3].Count);
            Assert.AreEqual(21, _ctx.UserPurse.Item.Coins[2].Count);
            Assert.AreEqual(30, _ctx.UserPurse.Item.Coins[1].Count);
            Assert.AreEqual(10, _ctx.UserPurse.Item.Coins[0].Count);
            Assert.AreEqual(0, _ctx.PurchaseInfo.Item.PayedSum);
            Assert.AreEqual(99, _ctx.CoffeMachinePurse.Item.Coins[3].Count);
            Assert.AreEqual(99, _ctx.CoffeMachinePurse.Item.Coins[2].Count);
            Assert.AreEqual(100, _ctx.CoffeMachinePurse.Item.Coins[1].Count);
            Assert.AreEqual(100, _ctx.CoffeMachinePurse.Item.Coins[0].Count);
        }

        [TestMethod]
        public void TestGetChangeShouldReturn0_10__2_5__2_2__1_1()
        {
            _ctx.Init(String.Empty);
            _ctx.PurchaseInfo.Item.PayedSum = 15;
            _ctx.CoffeMachinePurse.Item.Coins[3].Count = 0;
            _ctx.CoffeMachinePurse.Item.Coins[2].Count = 2;
            _ctx.CoffeMachinePurse.Item.Coins[1].Count = 6;
            _ctx.CoffeMachinePurse.Item.Coins[0].Count = 7;
            var result = _controller.GetChange().Result;
            Assert.AreEqual(15, _ctx.UserPurse.Item.Coins[3].Count);
            Assert.AreEqual(22, _ctx.UserPurse.Item.Coins[2].Count);
            Assert.AreEqual(32, _ctx.UserPurse.Item.Coins[1].Count);
            Assert.AreEqual(11, _ctx.UserPurse.Item.Coins[0].Count);
            Assert.AreEqual(0, _ctx.PurchaseInfo.Item.PayedSum);
            Assert.AreEqual(0, _ctx.CoffeMachinePurse.Item.Coins[3].Count);
            Assert.AreEqual(0, _ctx.CoffeMachinePurse.Item.Coins[2].Count);
            Assert.AreEqual(4, _ctx.CoffeMachinePurse.Item.Coins[1].Count);
            Assert.AreEqual(6, _ctx.CoffeMachinePurse.Item.Coins[0].Count);
        }


        [TestMethod]
        public void TestGetGoodsShouldSucceed()
        {
            GoodsModel result = _controller.GetGoods().Result;
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.goods);
            Assert.AreEqual(4, result.goods.Length);
            Assert.AreEqual("tea", result.goods[0].item);
            Assert.AreEqual(13, result.goods[0].price);
            Assert.AreEqual(10, result.goods[0].count);
            Assert.AreEqual("coffee", result.goods[1].item);
            Assert.AreEqual(18, result.goods[1].price);
            Assert.AreEqual(20, result.goods[1].count);
            Assert.AreEqual("cappuccino", result.goods[2].item);
            Assert.AreEqual(21, result.goods[2].price);
            Assert.AreEqual(10, result.goods[2].count);
            Assert.AreEqual("juice", result.goods[3].item);
            Assert.AreEqual(35, result.goods[3].price);
            Assert.AreEqual(15, result.goods[3].count);
        }

        [TestMethod]
        public void TestGetPurchaseInfoShouldSuccess()
        {
            _ctx.PurchaseInfo.Item.PayedSum = 10;
            _ctx.Save(String.Empty);
            PurchaseInfoModel result = _controller.GetPurchaseInfo().Result;
            Assert.IsNotNull(result);
            Assert.AreEqual(10, result.Sum);
        }

        [TestMethod]
        public void TestMakeChangeShouldSuccess()
        {
            Double sum = 21;
            var coins = _controller.MakeChangeForTests(sum);
            Assert.IsNotNull(coins);
            Assert.AreEqual(4, coins.Coins.Count);
            Assert.AreEqual(2, coins.Coins.First(x => x.Value == "10").Count);
            Assert.AreEqual(1, coins.Coins.First(x => x.Value == "1").Count);
        }

        [TestMethod]
        [ExpectedException(typeof(NotEnoughtMoneyException))]
        public void TestMakeChangeShouldFailWhenNoMoneyKind1InMachine()
        {
            _ctx.Init(String.Empty);
            _ctx.PurchaseInfo.Item.PayedSum = 11;
            _ctx.CoffeMachinePurse.Item.Coins[3].Count = 10;
            _ctx.CoffeMachinePurse.Item.Coins[2].Count = 10;
            _ctx.CoffeMachinePurse.Item.Coins[1].Count = 10;
            _ctx.CoffeMachinePurse.Item.Coins[0].Count = 0;
            var result = _controller.MakeChangeForTests(11);
        }

        [TestMethod]
        public void TestMakeUpdatesInPursesShouldSuccess()
        {
            PurseBase coinsToChange = new TempPurse();
            coinsToChange.Coins.Add(new Coin() { Value = "10", Count = 1 });
            coinsToChange.Coins.Add(new Coin() { Value = "5", Count = 2 });
            coinsToChange.Coins.Add(new Coin() { Value = "2", Count = 3 });
            coinsToChange.Coins.Add(new Coin() { Value = "1", Count = 4 });
            _ctx.Init(String.Empty);
            _controller.MakeUpdatesInPursesForTests(coinsToChange);
            Assert.AreEqual(16, _ctx.UserPurse.Item.Coins[3].Count);
            Assert.AreEqual(22, _ctx.UserPurse.Item.Coins[2].Count);
            Assert.AreEqual(33, _ctx.UserPurse.Item.Coins[1].Count);
            Assert.AreEqual(14, _ctx.UserPurse.Item.Coins[0].Count);
            Assert.AreEqual(99, _ctx.CoffeMachinePurse.Item.Coins[3].Count);
            Assert.AreEqual(98, _ctx.CoffeMachinePurse.Item.Coins[2].Count);
            Assert.AreEqual(97, _ctx.CoffeMachinePurse.Item.Coins[1].Count);
            Assert.AreEqual(96, _ctx.CoffeMachinePurse.Item.Coins[0].Count);
            Assert.AreEqual(0, _ctx.PurchaseInfo.Item.PayedSum);
        }

        [TestMethod]
        public void TestGetChangeShouldFailBecauseThereIsNoRequiredCoinsForChange()
        {
            _ctx.Init(String.Empty);
            _ctx.PurchaseInfo.Item.PayedSum = 11;
            _ctx.CoffeMachinePurse.Item.Coins[3].Count = 10;
            _ctx.CoffeMachinePurse.Item.Coins[2].Count = 10;
            _ctx.CoffeMachinePurse.Item.Coins[1].Count = 10;
            _ctx.CoffeMachinePurse.Item.Coins[0].Count = 0;
            var result = _controller.GetChange().Result;
            Assert.AreEqual(15, _ctx.UserPurse.Item.Coins[3].Count);
            Assert.AreEqual(20, _ctx.UserPurse.Item.Coins[2].Count);
            Assert.AreEqual(30, _ctx.UserPurse.Item.Coins[1].Count);
            Assert.AreEqual(10, _ctx.UserPurse.Item.Coins[0].Count);
            Assert.AreEqual(11, _ctx.PurchaseInfo.Item.PayedSum);
            Assert.AreEqual(10, _ctx.CoffeMachinePurse.Item.Coins[3].Count);
            Assert.AreEqual(10, _ctx.CoffeMachinePurse.Item.Coins[2].Count);
            Assert.AreEqual(10, _ctx.CoffeMachinePurse.Item.Coins[1].Count);
            Assert.AreEqual(0, _ctx.CoffeMachinePurse.Item.Coins[0].Count);
            Assert.AreEqual("В автомате не хватает монет достоинством 1!",
                result.GetType().GetProperty("error").GetValue(result));
        }

        [TestMethod]
        public void TestGetChangeShouldFailWhenLowMoneyInMachine()
        {
            _ctx.Init(String.Empty);
            _ctx.PurchaseInfo.Item.PayedSum = 15;
            _ctx.CoffeMachinePurse.Item.Coins[3].Count = 0;
            _ctx.CoffeMachinePurse.Item.Coins[2].Count = 0;
            _ctx.CoffeMachinePurse.Item.Coins[1].Count = 2;
            _ctx.CoffeMachinePurse.Item.Coins[0].Count = 1;
            var result = _controller.GetChange().Result;
            Assert.AreEqual(15, _ctx.UserPurse.Item.Coins[3].Count);
            Assert.AreEqual(20, _ctx.UserPurse.Item.Coins[2].Count);
            Assert.AreEqual(30, _ctx.UserPurse.Item.Coins[1].Count);
            Assert.AreEqual(10, _ctx.UserPurse.Item.Coins[0].Count);
            Assert.AreEqual(15, _ctx.PurchaseInfo.Item.PayedSum);
            Assert.AreEqual(0, _ctx.CoffeMachinePurse.Item.Coins[3].Count);
            Assert.AreEqual(0, _ctx.CoffeMachinePurse.Item.Coins[2].Count);
            Assert.AreEqual(2, _ctx.CoffeMachinePurse.Item.Coins[1].Count);
            Assert.AreEqual(1, _ctx.CoffeMachinePurse.Item.Coins[0].Count);
            Assert.AreEqual("В автомате не хватает денег на сдачу!",
                result.GetType().GetProperty("error").GetValue(result));
        }
    }
}
