using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EdiTools.Tests
{
    [TestClass]
    public class EdiElementTest
    {
        [TestMethod]
        public void CreatingAnElement()
        {
            var element = new EdiElement();
            element[02] = "1234";
            element[04] = "5678";

            Assert.IsNull(element[01]);
            Assert.AreEqual("1234", element[02]);
            Assert.IsNull(element[03]);
            Assert.AreEqual("5678", element[04]);
            Assert.IsNull(element[05]);
            Assert.AreEqual(1234, element.Component(02).RealValue);
            Assert.IsNull(element.Component(03));
        }

        [TestMethod]
        public void UpdatingComponents()
        {
            var element = new EdiElement();
            element[01] = "ORIGINAL01";
            element[02] = "ORIGINAL02";
            element[03] = "ORIGINAL03";
            element[01] = "UPDATE01";
            element.Component(02).Value = "UPDATE02";
            element[03] = null;

            Assert.AreEqual("UPDATE01", element[01]);
            Assert.AreEqual("UPDATE02", element[02]);
            Assert.IsNull(element[03]);
        }

        [TestMethod]
        public void ConvertingAnElementToAString()
        {
            var element = new EdiElement();
            element.Repetitions.Add(new EdiRepetition("1234"));
            element.Repetitions.Add(new EdiRepetition("5678"));

            Assert.AreEqual("1234^5678", element.ToString());
        }

        [TestMethod]
        public void ConvertingAnElementToAStringWithSpecificSeparators()
        {
            var element = new EdiElement();
            element.Repetitions.Add(new EdiRepetition("1234"));
            element.Repetitions.Add(new EdiRepetition("5678"));
            var options = new EdiOptions {RepetitionSeparator = '^'};

            Assert.AreEqual("1234^5678", element.ToString(options));
        }
    }
}