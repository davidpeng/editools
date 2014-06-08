using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EdiTools.Tests
{
    [TestClass]
    public class EdiSegmentTest
    {
        [TestMethod]
        public void CreatingASegment()
        {
            var seg = new EdiSegment("SEG");
            seg[02] = "1234";
            seg.Element(04, new EdiElement("5678"));

            Assert.IsNull(seg[01]);
            Assert.AreEqual("1234", seg[02]);
            Assert.IsNull(seg[03]);
            Assert.AreEqual("5678", seg[04]);
            Assert.IsNull(seg[05]);
            Assert.AreEqual(1234, seg.Element(02).RealValue);
            Assert.IsNull(seg.Element(03));
        }

        [TestMethod]
        public void CreatingAUNASegment()
        {
            var seg = new EdiSegment("UNA");
            seg[01] = ":+.? ";

            Assert.AreEqual("UNA:+.? ’", seg.ToString(new EdiOptions { SegmentTerminator = '’' }));
        }

        [TestMethod]
        public void UpdatingElements()
        {
            var seg = new EdiSegment("SEG");
            seg[01] = "ORIGINAL01";
            seg[02] = "ORIGINAL02";
            seg[03] = "ORIGINAL03";
            seg[04] = "ORIGINAL04";
            seg[05] = "ORIGINAL05";
            seg[01] = "UPDATE01";
            seg.Element(02, new EdiElement("UPDATE02"));
            seg.Element(03).Value = "UPDATE03";
            seg[04] = null;
            seg.Element(05, null);

            Assert.AreEqual("UPDATE01", seg[01]);
            Assert.AreEqual("UPDATE02", seg[02]);
            Assert.AreEqual("UPDATE03", seg[03]);
            Assert.IsNull(seg[04]);
            Assert.IsNull(seg[05]);
        }

        [TestMethod]
        public void ConvertingASegmentToAString()
        {
            var segment = new EdiSegment("SEG");
            segment[02] = "1234";
            segment[04] = "5678";

            Assert.AreEqual("SEG**1234**5678\r", segment.ToString());
        }

        [TestMethod]
        public void ConvertingASegmentToAStringWithSpecificSeparators()
        {
            var segment = new EdiSegment("SEG");
            segment[02] = "1234";
            segment[04] = "5678";
            var options = new EdiOptions {SegmentTerminator = '~'};

            Assert.AreEqual("SEG**1234**5678~", segment.ToString(options));
        }
    }
}