using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EdiTools.Tests
{
    [TestClass]
    public class EdiRepetitionTest
    {
        [TestMethod]
        public void CreatingARepetition()
        {
            var repetition = new EdiRepetition();
            repetition[02] = "1234";
            repetition[04] = "5678";

            Assert.IsNull(repetition[01]);
            Assert.AreEqual("1234", repetition[02]);
            Assert.IsNull(repetition[03]);
            Assert.AreEqual("5678", repetition[04]);
            Assert.IsNull(repetition[05]);
            Assert.AreEqual(1234, repetition.Component(02).RealValue);
            Assert.IsNull(repetition.Component(03));
        }

        [TestMethod]
        public void UpdatingComponents()
        {
            var repetition = new EdiRepetition();
            repetition[01] = "ORIGINAL01";
            repetition[02] = "ORIGINAL02";
            repetition[03] = "ORIGINAL03";
            repetition[01] = "UPDATE01";
            repetition.Component(02).Value = "UPDATE02";
            repetition[03] = null;

            Assert.AreEqual("UPDATE01", repetition[01]);
            Assert.AreEqual("UPDATE02", repetition[02]);
            Assert.IsNull(repetition[03]);
        }

        [TestMethod]
        public void ConvertingARepetitionToAString()
        {
            var repetition = new EdiRepetition();
            repetition[02] = "1234";
            repetition[04] = "5678";

            Assert.AreEqual(">1234>>5678", repetition.ToString());
        }

        [TestMethod]
        public void ConvertingARepetitionToAStringWithSpecificSeparators()
        {
            var repetition = new EdiRepetition();
            repetition[02] = "1234";
            repetition[04] = "5678";
            var options = new EdiOptions {ComponentSeparator = '<'};

            Assert.AreEqual("<1234<<5678", repetition.ToString(options));
        }
    }
}