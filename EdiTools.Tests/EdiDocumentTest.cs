using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EdiTools.Tests
{
    [TestClass]
    public class EdiDocumentTest
    {
        [TestMethod]
        public void ReadingASegment()
        {
            var options = new EdiOptions
                              {
                                  ElementSeparator = '*',
                                  SegmentTerminator = '~'
                              };
            EdiDocument document = EdiDocument.Parse("ST*997*0001~", options);

            Assert.AreEqual(1, document.Segments.Count);
            Assert.AreEqual("ST", document.Segments[0].Id);
            Assert.AreEqual(2, document.Segments[0].Elements.Count);
            Assert.AreEqual("997", document.Segments[0].Elements[0].Value);
            Assert.AreEqual("0001", document.Segments[0].Elements[1].Value);
        }

        [TestMethod]
        public void ReadingTwoSegments()
        {
            var options = new EdiOptions
                              {
                                  ElementSeparator = '*',
                                  SegmentTerminator = '~'
                              };
            EdiDocument document = EdiDocument.Parse("ST*997*0001~SE*1*0001~", options);

            Assert.AreEqual(2, document.Segments.Count);
            Assert.AreEqual("ST", document.Segments[0].Id);
            Assert.AreEqual(2, document.Segments[0].Elements.Count);
            Assert.AreEqual("997", document.Segments[0].Elements[0].Value);
            Assert.AreEqual("0001", document.Segments[0].Elements[1].Value);
            Assert.AreEqual("SE", document.Segments[1].Id);
            Assert.AreEqual(2, document.Segments[1].Elements.Count);
            Assert.AreEqual("1", document.Segments[1].Elements[0].Value);
            Assert.AreEqual("0001", document.Segments[1].Elements[1].Value);
        }

        [TestMethod]
        public void GuessingSeparators()
        {
            EdiDocument document =
                EdiDocument.Parse(
                    "ISA*00*          *00*          *ZZ*SENDER         *ZZ*RECEIVER       *120101*0000*U*00401*000000001*0*P*>~IEA*0*000000001~");

            Assert.AreEqual(2, document.Segments.Count);
            Assert.AreEqual("ISA", document.Segments[0].Id);
            Assert.AreEqual(16, document.Segments[0].Elements.Count);
            Assert.AreEqual("SENDER         ", document.Segments[0].Elements[5].Value);
            Assert.AreEqual(">", document.Segments[0].Elements[15].Value);
            Assert.AreEqual("IEA", document.Segments[1].Id);
            Assert.AreEqual(2, document.Segments[1].Elements.Count);
            Assert.AreEqual("0", document.Segments[1].Elements[0].Value);
            Assert.AreEqual("000000001", document.Segments[1].Elements[1].Value);
        }

        [TestMethod]
        public void IgnoringExtraWhiteSpace()
        {
            string edi = new StringBuilder()
                .AppendLine(
                    "ISA*00*          *00*          *ZZ*SENDER         *ZZ*RECEIVER       *120101*0000*U*00401*000000001*0*P*>~")
                .AppendLine("IEA*0*000000001~")
                .ToString();
            EdiDocument document = EdiDocument.Parse(edi);

            Assert.AreEqual(2, document.Segments.Count);
            Assert.AreEqual("ISA", document.Segments[0].Id);
            Assert.AreEqual(16, document.Segments[0].Elements.Count);
            Assert.AreEqual("SENDER         ", document.Segments[0].Elements[5].Value);
            Assert.AreEqual(">", document.Segments[0].Elements[15].Value);
            Assert.AreEqual("IEA", document.Segments[1].Id);
            Assert.AreEqual(2, document.Segments[1].Elements.Count);
            Assert.AreEqual("0", document.Segments[1].Elements[0].Value);
            Assert.AreEqual("000000001", document.Segments[1].Elements[1].Value);
        }

        [TestMethod]
        public void ReadingComponents()
        {
            EdiDocument document =
                EdiDocument.Parse(
                    "ISA*00*          *00*          *ZZ*SENDER         *ZZ*RECEIVER       *120101*0000*U*00401*000000001*0*P*>~SEG*COMPONENT1>COMPONENT2*COMPONENT3>COMPONENT4>COMPONENT5~");

            Assert.AreEqual(2, document.Segments[1].Elements.Count);
            Assert.AreEqual(2, document.Segments[1].Elements[0].Components.Count);
            Assert.AreEqual("COMPONENT1", document.Segments[1].Elements[0].Components[0].Value);
            Assert.AreEqual("COMPONENT2", document.Segments[1].Elements[0].Components[1].Value);
            Assert.AreEqual(3, document.Segments[1].Elements[1].Components.Count);
            Assert.AreEqual("COMPONENT3", document.Segments[1].Elements[1].Components[0].Value);
            Assert.AreEqual("COMPONENT4", document.Segments[1].Elements[1].Components[1].Value);
            Assert.AreEqual("COMPONENT5", document.Segments[1].Elements[1].Components[2].Value);
        }

        [TestMethod]
        public void ReadingRepetitions()
        {
            EdiDocument document =
                EdiDocument.Parse(
                    "ISA*00*          *00*          *ZZ*SENDER         *ZZ*RECEIVER       *120101*0000*^*00402*000000001*0*P*>~SEG*REPETITION1^COMPONENT1>COMPONENT2~");

            Assert.AreEqual(1, document.Segments[1].Elements.Count);
            Assert.AreEqual(2, document.Segments[1].Elements[0].Repetitions.Count);
            Assert.AreEqual("REPETITION1", document.Segments[1].Elements[0].Repetitions[0].Value);
            Assert.AreEqual(2, document.Segments[1].Elements[0].Repetitions[1].Components.Count);
            Assert.AreEqual("COMPONENT1", document.Segments[1].Elements[0].Repetitions[1].Components[0].Value);
            Assert.AreEqual("COMPONENT2", document.Segments[1].Elements[0].Repetitions[1].Components[1].Value);
        }

        [TestMethod]
        public void IgnoringRepetitionSeparatorWhenLessThanVersion4020()
        {
            EdiDocument document =
                EdiDocument.Parse(
                    "ISA*00*          *00*          *ZZ*SENDER         *ZZ*RECEIVER       *120101*0000*^*00401*000000001*0*P*>~SEG*REPETITION1^COMPONENT1>COMPONENT2~");

            Assert.AreEqual(1, document.Segments[1].Elements.Count);
            Assert.AreEqual(2, document.Segments[1].Elements[0].Components.Count);
            Assert.AreEqual("REPETITION1^COMPONENT1", document.Segments[1].Elements[0].Components[0].Value);
            Assert.AreEqual("COMPONENT2", document.Segments[1].Elements[0].Components[1].Value);
        }

        [TestMethod]
        public void IgnoringAlphaNumericRepetitionSeparator()
        {
            EdiDocument document =
                EdiDocument.Parse(
                    "ISA*00*          *00*          *ZZ*SENDER         *ZZ*RECEIVER       *120101*0000*U*00402*000000001*0*P*>~SEG*VALUE1>VALUE2~");

            Assert.AreEqual(1, document.Segments[1].Elements.Count);
            Assert.AreEqual(2, document.Segments[1].Elements[0].Components.Count);
            Assert.AreEqual("VALUE1", document.Segments[1].Elements[0].Components[0].Value);
            Assert.AreEqual("VALUE2", document.Segments[1].Elements[0].Components[1].Value);
        }

        [TestMethod]
        public void Saving()
        {
            const string edi =
                "ISA*00*          *00*          *ZZ*SENDER         *ZZ*RECEIVER       *120101*0000*^*00402*000000001*0*P*>~SEG*REPETITION1^COMPONENT1>COMPONENT2~";
            EdiDocument document = EdiDocument.Parse(edi);
            var buffer = new StringWriter();
            document.Save(buffer);

            Assert.AreEqual(edi, buffer.ToString());
        }

        [TestMethod]
        public void DetectingRepetitionAndComponentSeparatorsWhenSaving()
        {
            string edi = new StringBuilder()
                .Append(
                    "ISA*00*          *00*          *ZZ*SENDER         *ZZ*RECEIVER       *120101*0000*^*00402*000000001*0*P*>~")
                .Append("SEG*REPETITION1^COMPONENT1>COMPONENT2~")
                .Append(
                    "ISA*00*          *00*          *ZZ*SENDER         *ZZ*RECEIVER       *120101*0000*|*00402*000000001*0*P*<~")
                .Append("SEG*REPETITION1|COMPONENT1<COMPONENT2~")
                .ToString();
            EdiDocument document = EdiDocument.Parse(edi);
            var buffer = new StringWriter();
            document.Save(buffer);

            Assert.AreEqual(edi, buffer.ToString());
        }

        [TestMethod]
        [ExpectedException(typeof (FormatException))]
        public void SavingAValueContainingASeparator()
        {
            var options = new EdiOptions {SegmentTerminator = '~', ElementSeparator = '*'};
            var document = new EdiDocument(options);
            var segment = new EdiSegment("SEG");
            segment[01] = document.Options.ElementSeparator.ToString();
            document.Segments.Add(segment);
            var buffer = new StringWriter();
            document.Save(buffer);
        }

        [TestMethod]
        public void GettingTransactionSets()
        {
            string edi = new StringBuilder()
                .AppendLine(
                    "ISA*00*          *00*          *ZZ*SENDER         *ZZ*RECEIVER       *120101*0000*U*00401*000000001*0*P*>~")
                .AppendLine("GS********1~")
                .AppendLine("ST**0001~")
                .AppendLine("AK1~")
                .AppendLine("SE~")
                .AppendLine("SE~")
                .AppendLine("GE~")
                .AppendLine("IEA~")
                .AppendLine("ST**0002~")
                .AppendLine("SE~")
                .AppendLine(
                    "ISA*00*          *00*          *ZZ*SENDER         *ZZ*RECEIVER       *120101*0000*U*00401*000000002*0*P*>~")
                .AppendLine("GS********2~")
                .AppendLine("AK2~")
                .AppendLine("ST**0003~")
                .AppendLine("AK3~")
                .AppendLine("ST**0004~")
                .AppendLine("GE~")
                .AppendLine("IEA~")
                .ToString();
            EdiDocument document = EdiDocument.Parse(edi);
            IList<EdiTransactionSet> transactionSets = document.TransactionSets;

            Assert.AreEqual(4, transactionSets.Count);

            Assert.AreEqual("000000001", transactionSets[0].Isa[13]);
            Assert.AreEqual("1", transactionSets[0].Gs[8]);
            Assert.AreEqual(3, transactionSets[0].Segments.Count);
            Assert.AreEqual("0001", transactionSets[0].Segments[0][02]);
            Assert.AreEqual("AK1", transactionSets[0].Segments[1].Id);
            Assert.AreEqual("SE", transactionSets[0].Segments[2].Id);

            Assert.IsNull(transactionSets[1].Isa);
            Assert.IsNull(transactionSets[1].Gs);
            Assert.AreEqual(2, transactionSets[1].Segments.Count);
            Assert.AreEqual("0002", transactionSets[1].Segments[0][02]);
            Assert.AreEqual("SE", transactionSets[1].Segments[1].Id);

            Assert.AreEqual("000000002", transactionSets[2].Isa[13]);
            Assert.AreEqual("2", transactionSets[2].Gs[8]);
            Assert.AreEqual(2, transactionSets[2].Segments.Count);
            Assert.AreEqual("0003", transactionSets[2].Segments[0][02]);
            Assert.AreEqual("AK3", transactionSets[2].Segments[1].Id);

            Assert.AreEqual("000000002", transactionSets[3].Isa[13]);
            Assert.AreEqual("2", transactionSets[3].Gs[8]);
            Assert.AreEqual(3, transactionSets[3].Segments.Count);
            Assert.AreEqual("0004", transactionSets[3].Segments[0][02]);
            Assert.AreEqual("GE", transactionSets[3].Segments[1].Id);
            Assert.AreEqual("IEA", transactionSets[3].Segments[2].Id);
        }

        [TestMethod]
        public void LoadingAnXml()
        {
            XDocument xml = XDocument.Parse(new StringBuilder()
                                                .AppendLine("<mapping>")
                                                .AppendLine("    <st>")
                                                .AppendLine(
                                                    "        <st01 type=\"n1\" definition=\"acknowledgment\">99.7</st01>")
                                                .AppendLine("        <st01>")
                                                .AppendLine("            <st0101>850</st0101>")
                                                .AppendLine("            <st0102>810</st0102>")
                                                .AppendLine("        </st01>")
                                                .AppendLine("        <st02>")
                                                .AppendLine(
                                                    "            <st0201 type=\"n3\" definition=\"def\">1.234</st0201>")
                                                .AppendLine("            <st0203>5678</st0203>")
                                                .AppendLine("        </st02>")
                                                .AppendLine("        <st04>123</st04>")
                                                .AppendLine("    </st>")
                                                .AppendLine("</mapping>")
                                                .ToString());
            EdiDocument document = EdiDocument.LoadXml(xml);
            document.Options.SegmentTerminator = '~';
            document.Options.ElementSeparator = '*';
            document.Options.ComponentSeparator = '>';
            document.Options.RepetitionSeparator = '^';
            string edi = document.ToString();

            Assert.AreEqual("ST*997^850>810*1234>>5678**123~", edi);
        }
    }
}