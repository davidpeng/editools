using System;
using System.Globalization;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EdiTools.Tests
{
    [TestClass]
    public class EdiValueTest
    {
        [ClassInitialize]
        public static void SetUp(TestContext context)
        {
            SetCulture("en");
        }

        private static void SetCulture(String culture)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
        }

        [TestMethod]
        public void GettingTheDateValueOfAnElement()
        {
            Assert.AreEqual(new DateTime(2013, 2, 23), new EdiElement("130223").DateValue);
            Assert.AreEqual(new DateTime(2013, 2, 23), new EdiElement("20130223").DateValue);
        }

        [TestMethod]
        public void GettingTheTimeValueOfAnElement()
        {
            Assert.AreEqual(DateTime.Parse("3:24"), new EdiElement("0324").TimeValue);
            Assert.AreEqual(DateTime.Parse("3:24:22"), new EdiElement("032422").TimeValue);
            Assert.AreEqual(DateTime.Parse("3:24:22.150"), new EdiElement("03242215").TimeValue);
        }

        [TestMethod]
        public void GettingTheNumericValueOfAnElement()
        {
            Assert.AreEqual(123, new EdiElement("123").NumericValue(0));
            Assert.AreEqual(-123, new EdiElement("-123").NumericValue(0));
            Assert.AreEqual(12.3m, new EdiElement("123").NumericValue(1));
            Assert.AreEqual(-12.3m, new EdiElement("-123").NumericValue(1));
            Assert.AreEqual(1.23m, new EdiElement("123").NumericValue(2));
            Assert.AreEqual(0.123m, new EdiElement("123").NumericValue(3));
            Assert.AreEqual(-0.123m, new EdiElement("-123").NumericValue(3));
        }

        [TestMethod]
        public void GettingTheNumericValueOfAnElementInNonEnCulture()
        {
            SetCulture("fr");
            Assert.AreEqual(123, new EdiElement("123").NumericValue(0));
            Assert.AreEqual(-123, new EdiElement("-123").NumericValue(0));
            Assert.AreEqual(12.3m, new EdiElement("123").NumericValue(1));
            Assert.AreEqual(-12.3m, new EdiElement("-123").NumericValue(1));
            Assert.AreEqual(1.23m, new EdiElement("123").NumericValue(2));
            Assert.AreEqual(0.123m, new EdiElement("123").NumericValue(3));
            Assert.AreEqual(-0.123m, new EdiElement("-123").NumericValue(3));
        }

        [TestMethod]
        public void GettingTheRealValueOfAnElement()
        {
            Assert.AreEqual(123, new EdiElement("123").RealValue);
            Assert.AreEqual(-123, new EdiElement("-123").RealValue);
            Assert.AreEqual(12.3m, new EdiElement("12.3").RealValue);
            Assert.AreEqual(-12.3m, new EdiElement("-12.3").RealValue);
            Assert.AreEqual(1.23m, new EdiElement("1.23").RealValue);
            Assert.AreEqual(0.123m, new EdiElement(".123").RealValue);
            Assert.AreEqual(-0.123m, new EdiElement("-.123").RealValue);
            Assert.AreEqual(1.23m, new EdiElement("1,23").RealValue);
        }

        [TestMethod]
        public void GettingTheRealValueOfAnElementInNonEnCulture()
        {
            SetCulture("fr");
            Assert.AreEqual(123, new EdiElement("123").RealValue);
            Assert.AreEqual(-123, new EdiElement("-123").RealValue);
            Assert.AreEqual(12.3m, new EdiElement("12.3").RealValue);
            Assert.AreEqual(-12.3m, new EdiElement("-12.3").RealValue);
            Assert.AreEqual(1.23m, new EdiElement("1.23").RealValue);
            Assert.AreEqual(0.123m, new EdiElement(".123").RealValue);
            Assert.AreEqual(-0.123m, new EdiElement("-.123").RealValue);
            Assert.AreEqual(1.23m, new EdiElement("1,23").RealValue);
        }

        [TestMethod]
        public void GettingTheIsoDateOfAnElement()
        {
            Assert.AreEqual("2013-02-23", new EdiElement("130223").IsoDate);
            Assert.AreEqual("2013-02-23", new EdiElement("20130223").IsoDate);
        }

        [TestMethod]
        public void GettingTheIsoTimeOfAnElement()
        {
            Assert.AreEqual("03:24", new EdiElement("0324").IsoTime);
            Assert.AreEqual("03:24:22", new EdiElement("032422").IsoTime);
            Assert.AreEqual("03:24:22.15", new EdiElement("03242215").IsoTime);
        }

        [TestMethod]
        public void FormattingADate()
        {
            var date = new DateTime(2013, 2, 23);
            Assert.AreEqual("130223", EdiValue.Date(6, date));
            Assert.AreEqual("20130223", EdiValue.Date(8, date));
        }

        [TestMethod]
        public void FormattingATime()
        {
            DateTime time = DateTime.Parse("3:24:22.150");
            Assert.AreEqual("0324", EdiValue.Time(4, time));
            Assert.AreEqual("032422", EdiValue.Time(6, time));
            Assert.AreEqual("03242215", EdiValue.Time(8, time));
        }

        [TestMethod]
        public void FormattingANumeric()
        {
            Assert.AreEqual("123", EdiValue.Numeric(0, 123));
            Assert.AreEqual("-123", EdiValue.Numeric(0, -123));
            Assert.AreEqual("1230", EdiValue.Numeric(1, 123));
            Assert.AreEqual("-1230", EdiValue.Numeric(1, -123));
            Assert.AreEqual("123", EdiValue.Numeric(1, 12.3m));
            Assert.AreEqual("-123", EdiValue.Numeric(1, -12.3m));
            Assert.AreEqual("1230", EdiValue.Numeric(2, 12.3m));
            Assert.AreEqual("-1230", EdiValue.Numeric(2, -12.3m));
            Assert.AreEqual("12", EdiValue.Numeric(0, 12.3m));
            Assert.AreEqual("-12", EdiValue.Numeric(0, -12.3m));
            Assert.AreEqual("0", EdiValue.Numeric(2, 0));
            Assert.AreEqual("1", EdiValue.Numeric(2, 0.01m));
            Assert.AreEqual("-1", EdiValue.Numeric(2, -0.01m));
        }

        [TestMethod]
        public void FormattingANumericInNonEnCulture()
        {
            SetCulture("fr");
            Assert.AreEqual("123", EdiValue.Numeric(0, 123));
            Assert.AreEqual("-123", EdiValue.Numeric(0, -123));
            Assert.AreEqual("1230", EdiValue.Numeric(1, 123));
            Assert.AreEqual("-1230", EdiValue.Numeric(1, -123));
            Assert.AreEqual("123", EdiValue.Numeric(1, 12.3m));
            Assert.AreEqual("-123", EdiValue.Numeric(1, -12.3m));
            Assert.AreEqual("1230", EdiValue.Numeric(2, 12.3m));
            Assert.AreEqual("-1230", EdiValue.Numeric(2, -12.3m));
            Assert.AreEqual("12", EdiValue.Numeric(0, 12.3m));
            Assert.AreEqual("-12", EdiValue.Numeric(0, -12.3m));
            Assert.AreEqual("0", EdiValue.Numeric(2, 0));
            Assert.AreEqual("1", EdiValue.Numeric(2, 0.01m));
            Assert.AreEqual("-1", EdiValue.Numeric(2, -0.01m));
        }

        [TestMethod]
        public void FormattingAReal()
        {
            Assert.AreEqual("123", EdiValue.Real(123));
            Assert.AreEqual("-123", EdiValue.Real(-123));
            Assert.AreEqual("12.3", EdiValue.Real(12.3m));
            Assert.AreEqual("-12.3", EdiValue.Real(-12.3m));
            Assert.AreEqual("0.123", EdiValue.Real(0.123m));
            Assert.AreEqual("-0.123", EdiValue.Real(-0.123m));
        }
    }
}