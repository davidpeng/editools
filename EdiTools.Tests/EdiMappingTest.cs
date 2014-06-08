using System.Text;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EdiTools.Tests
{
    [TestClass]
    public class EdiMappingTest
    {
        [TestMethod]
        public void MappingBlankEdiWithBlankMapping()
        {
            var segments = new EdiSegment[0];
            EdiMapping mapping = EdiMapping.Parse("<mapping/>");
            XDocument actual = mapping.Map(segments);

            XDocument expected = XDocument.Parse("<mapping/>");
            Assert.IsTrue(XNode.DeepEquals(expected, actual));
        }

        [TestMethod]
        public void MappingOneSegmentWithBlankMapping()
        {
            EdiDocument edi = EdiDocument.Parse("ST*997^850>810*1234>>5678**123~", new EdiOptions {RepetitionSeparator = '^', ComponentSeparator = '>'});
            EdiMapping mapping = EdiMapping.Parse("<mapping/>");
            XDocument actual = mapping.Map(edi.Segments);

            XDocument expected = XDocument.Parse(new StringBuilder()
                                                     .AppendLine("<mapping>")
                                                     .AppendLine("    <ST>")
                                                     .AppendLine("        <ST01>997</ST01>")
                                                     .AppendLine("        <ST01>")
                                                     .AppendLine("            <ST0101>850</ST0101>")
                                                     .AppendLine("            <ST0102>810</ST0102>")
                                                     .AppendLine("        </ST01>")
                                                     .AppendLine("        <ST02>")
                                                     .AppendLine("            <ST0201>1234</ST0201>")
                                                     .AppendLine("            <ST0203>5678</ST0203>")
                                                     .AppendLine("        </ST02>")
                                                     .AppendLine("        <ST04>123</ST04>")
                                                     .AppendLine("    </ST>")
                                                     .AppendLine("</mapping>")
                                                     .ToString());
            Assert.IsTrue(XNode.DeepEquals(expected, actual));
        }

        [TestMethod]
        public void MappingAnExpectedSegment()
        {
            EdiDocument edi = EdiDocument.Parse("ST*997^850>810*1234>>5678**123~", new EdiOptions {RepetitionSeparator = '^', ComponentSeparator = '>'});
            EdiMapping mapping = EdiMapping.Parse(new StringBuilder()
                                                      .AppendLine("<mapping>")
                                                      .AppendLine("    <st>")
                                                      .AppendLine("        <st01 type=\"n1\">")
                                                      .AppendLine("            <option definition=\"purchase order\">850</option>")
                                                      .AppendLine("            <option definition=\"acknowledgment\">997</option>")
                                                      .AppendLine("        </st01>")
                                                      .AppendLine("        <st02 type=\"n2\">")
                                                      .AppendLine("            <st0201 type=\"n3\">")
                                                      .AppendLine("                <option definition=\"def\">1234</option>")
                                                      .AppendLine("            </st0201>")
                                                      .AppendLine("        </st02>")
                                                      .AppendLine("    </st>")
                                                      .AppendLine("</mapping>")
                                                      .ToString());
            XDocument actual = mapping.Map(edi.Segments);

            XDocument expected = XDocument.Parse(new StringBuilder()
                                                     .AppendLine("<mapping>")
                                                     .AppendLine("    <st>")
                                                     .AppendLine("        <st01 type=\"n1\" definition=\"acknowledgment\">99.7</st01>")
                                                     .AppendLine("        <st01>")
                                                     .AppendLine("            <st0101>850</st0101>")
                                                     .AppendLine("            <st0102>810</st0102>")
                                                     .AppendLine("        </st01>")
                                                     .AppendLine("        <st02>")
                                                     .AppendLine("            <st0201 type=\"n3\" definition=\"def\">1.234</st0201>")
                                                     .AppendLine("            <st0203>5678</st0203>")
                                                     .AppendLine("        </st02>")
                                                     .AppendLine("        <st04>123</st04>")
                                                     .AppendLine("    </st>")
                                                     .AppendLine("</mapping>")
                                                     .ToString());
            Assert.IsTrue(XNode.DeepEquals(expected, actual));
        }

        [TestMethod]
        public void MappingAHierarchy()
        {
            EdiDocument edi = EdiDocument.Parse(new StringBuilder()
                                                    .AppendLine("ST~")
                                                    .AppendLine("HL***S~")
                                                    .AppendLine("TD1~")
                                                    .AppendLine("N1~")
                                                    .AppendLine("N3~")
                                                    .AppendLine("N4~")
                                                    .AppendLine("N1~")
                                                    .AppendLine("N1~")
                                                    .AppendLine("HL***O~")
                                                    .AppendLine("PRF~")
                                                    .AppendLine("TD1~")
                                                    .AppendLine("N1~")
                                                    .AppendLine("HL***I~")
                                                    .AppendLine("HL***I~")
                                                    .AppendLine("HL***O~")
                                                    .AppendLine("PRF~")
                                                    .AppendLine("TD1~")
                                                    .AppendLine("N1~")
                                                    .AppendLine("HL***I~")
                                                    .AppendLine("CTT~")
                                                    .AppendLine("SE~")
                                                    .ToString(),
                                                new EdiOptions
                                                    {
                                                        ElementSeparator = '*',
                                                        RepetitionSeparator = '^',
                                                        ComponentSeparator = '>'
                                                    });
            EdiMapping mapping = EdiMapping.Parse(new StringBuilder()
                                                      .AppendLine("<mapping>")
                                                      .AppendLine("    <loop>")
                                                      .AppendLine("        <hl>")
                                                      .AppendLine("            <hl03 restrict=\"true\">")
                                                      .AppendLine("                <option>s</option>")
                                                      .AppendLine("            </hl03>")
                                                      .AppendLine("        </hl>")
                                                      .AppendLine("        <loop>")
                                                      .AppendLine("            <n1/>")
                                                      .AppendLine("        </loop>")
                                                      .AppendLine("    </loop>")
                                                      .AppendLine("    <loop>")
                                                      .AppendLine("        <hl>")
                                                      .AppendLine("            <hl03 restrict=\"true\">")
                                                      .AppendLine("                <option>o</option>")
                                                      .AppendLine("            </hl03>")
                                                      .AppendLine("        </hl>")
                                                      .AppendLine("        <td1/>")
                                                      .AppendLine("    </loop>")
                                                      .AppendLine("    <loop>")
                                                      .AppendLine("        <hl>")
                                                      .AppendLine("            <hl03 restrict=\"true\">")
                                                      .AppendLine("                <option>i</option>")
                                                      .AppendLine("            </hl03>")
                                                      .AppendLine("        </hl>")
                                                      .AppendLine("    </loop>")
                                                      .AppendLine("    <ctt/>")
                                                      .AppendLine("</mapping>")
                                                      .ToString());
            XDocument actual = mapping.Map(edi.Segments);

            XDocument expected = XDocument.Parse(new StringBuilder()
                                                     .AppendLine("<mapping>")
                                                     .AppendLine("    <ST/>")
                                                     .AppendLine("    <loop>")
                                                     .AppendLine("        <hl>")
                                                     .AppendLine("            <hl03>S</hl03>")
                                                     .AppendLine("        </hl>")
                                                     .AppendLine("        <TD1/>")
                                                     .AppendLine("        <loop>")
                                                     .AppendLine("            <n1/>")
                                                     .AppendLine("            <N3/>")
                                                     .AppendLine("            <N4/>")
                                                     .AppendLine("        </loop>")
                                                     .AppendLine("        <loop>")
                                                     .AppendLine("            <n1/>")
                                                     .AppendLine("        </loop>")
                                                     .AppendLine("        <loop>")
                                                     .AppendLine("            <n1/>")
                                                     .AppendLine("        </loop>")
                                                     .AppendLine("    </loop>")
                                                     .AppendLine("    <loop>")
                                                     .AppendLine("        <hl>")
                                                     .AppendLine("            <hl03>O</hl03>")
                                                     .AppendLine("        </hl>")
                                                     .AppendLine("        <PRF/>")
                                                     .AppendLine("        <td1/>")
                                                     .AppendLine("        <N1/>")
                                                     .AppendLine("    </loop>")
                                                     .AppendLine("    <loop>")
                                                     .AppendLine("        <hl>")
                                                     .AppendLine("            <hl03>I</hl03>")
                                                     .AppendLine("        </hl>")
                                                     .AppendLine("    </loop>")
                                                     .AppendLine("    <loop>")
                                                     .AppendLine("        <hl>")
                                                     .AppendLine("            <hl03>I</hl03>")
                                                     .AppendLine("        </hl>")
                                                     .AppendLine("    </loop>")
                                                     .AppendLine("    <loop>")
                                                     .AppendLine("        <hl>")
                                                     .AppendLine("            <hl03>O</hl03>")
                                                     .AppendLine("        </hl>")
                                                     .AppendLine("        <PRF/>")
                                                     .AppendLine("        <td1/>")
                                                     .AppendLine("        <N1/>")
                                                     .AppendLine("    </loop>")
                                                     .AppendLine("    <loop>")
                                                     .AppendLine("        <hl>")
                                                     .AppendLine("            <hl03>I</hl03>")
                                                     .AppendLine("        </hl>")
                                                     .AppendLine("    </loop>")
                                                     .AppendLine("    <ctt/>")
                                                     .AppendLine("    <SE/>")
                                                     .AppendLine("</mapping>")
                                                     .ToString());
            Assert.IsTrue(XNode.DeepEquals(expected, actual));
        }
    }
}