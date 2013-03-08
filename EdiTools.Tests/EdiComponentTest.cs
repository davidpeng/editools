using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EdiTools.Tests
{
    [TestClass]
    public class EdiComponentTest
    {
        [TestMethod]
        public void ConvertingAComponentToAString()
        {
            var component = new EdiComponent("value");
            Assert.AreEqual("value", component.ToString());
        }

        [TestMethod]
        [ExpectedException(typeof (FormatException))]
        public void StringifyingAComponentContainingADefaultSeparator()
        {
            var component = new EdiComponent("a>b");
            Assert.AreEqual("a>b", component.ToString());
        }

        [TestMethod]
        [ExpectedException(typeof (FormatException))]
        public void StringifyingAComponentContainingASpecificSeparator()
        {
            var component = new EdiComponent("a>b");
            var options = new EdiOptions {ComponentSeparator = '>'};
            Assert.AreEqual("a>b", component.ToString(options));
        }
    }
}