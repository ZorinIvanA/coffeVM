using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VirtualCoffee.Repository;
using System.Collections;
using System.Collections.Generic;

namespace VirtualCoffee.Tests
{
    public class PurseRepositoryBaseForTests : PurseRepositoryBase
    {
        public void InitDictionary()
        {
            _coins = new Dictionary<String, Int32>();
        }

        public IDictionary<String, Int32> GetCoins()
        {
            return _coins;
        }

        public void FillPurseWithCoins()
        {
            this.Coins.Add(new Coin1().Value, 10);
            this.Coins.Add(new Coin2().Value, 20);
            this.Coins.Add(new Coin5().Value, 30);
            this.Coins.Add(new Coin10().Value, 15);
        }

    }

    [TestClass]
    public class PurseBaseTests
    {
        PurseRepositoryBaseForTests _repository;

        [TestInitialize]
        public void TestsInitialize()
        {
            _repository = new PurseRepositoryBaseForTests();
        }


        [TestMethod]
        public void TestCoinsShouldGetDictionaryNormally()
        {
            _repository.InitDictionary();
            Assert.IsNotNull(_repository.Coins);
        }

        [TestMethod]
        public void TestCoinsShouldGetDictionaryWhen_coinsIsNull()
        {
            Assert.IsNotNull(_repository.Coins);
        }

        [TestMethod]
        public void TestCoinsShouldSetValueNormally()
        {
            _repository.Coins = new Dictionary<String, Int32>();
            Assert.IsNotNull(_repository.GetCoins());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestCoinsShouldThrowExceptionIfValueIsNull()
        {
            _repository.Coins = null;
        }

        [TestMethod]
        public void TestPutCoinShouldIncreaseCoins()
        {
            _repository.FillPurseWithCoins();
            _repository.PutCoin(new Coin1());
            Int32 result = _repository.Coins["1"];
            Assert.AreEqual(11, result);
        }

        [TestMethod]
        public void TestGetCoinShouldDecreaseCoins()
        {
            _repository.FillPurseWithCoins();
            _repository.GetCoin(new Coin1());
            Int32 result = _repository.Coins["1"];
            Assert.AreEqual(9, result);
        }
    }
}
