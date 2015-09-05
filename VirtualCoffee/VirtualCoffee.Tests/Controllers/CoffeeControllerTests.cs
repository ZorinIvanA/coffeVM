using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VirtualCoffee.Controllers;
using VirtualCoffee.Repository;
using System.Collections.Generic;

namespace VirtualCoffee.Tests.Controllers
{
    public class VirtualDataSetsMock
    { 
    }


    public class CoffeMachineControllerForTests : CoffeeMachineController
    {
   
    }

    [TestClass]
    public class CoffeeControllerTests
    {
        CoffeMachineControllerForTests _controller;

        [TestInitialize]
        public void InitializeTests()
        {

        }
    }
}
