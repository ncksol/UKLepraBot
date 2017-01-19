using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UKLepraBot.Tests
{
    [TestClass]
    public class HuifyingTests
    {
        [TestMethod]
        public void TestExceptionOnShortMessages()
        {
            var huifyied = Huify.HuifyMe("д");
            huifyied = Huify.HuifyMe("да");
            huifyied = Huify.HuifyMe("даг");
            huifyied = Huify.HuifyMe("даги");
        }
    }
}
