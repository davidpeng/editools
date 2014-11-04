EdiTools
========

EdiTools is a .NET library used to parse EDI, convert EDI into XML, and write out EDI.

You can install it as a [NuGet package](http://nuget.org/packages/EdiTools/).

Loading EDI
-----------

Given EDI in a file 850.txt:

```
ISA*00*          *00*          *ZZ*SENDER         *ZZ*RECEIVER       *130101*0000*U*00400*000000001*0*P*>~
GS*PO*SENDER*RECEIVER*20130101*0000*1*X*004010~
ST*850*0001~
BEG*00*NE*Z9999999**20130101~
DTM*064*20130101~
DTM*063*20130108~
N1*ST**92*9999~
PO1*1*6*EA*19.99*PE*UP*999999999993~
CTT*1*6~
SE*8*0001~
GE*1*1~
IEA*1*000000001~
```

We can load the file and access the EDI data with the following code:

```csharp
EdiDocument ediDocument = EdiDocument.Load("850.txt");
ediDocument.Segments[1].Id.IsEqualTo("GS");
ediDocument.Segments[3][05].IsEqualTo("20130101");
ediDocument.Segments[3].Element(05).DateValue.IsEqualTo(new DateTime(2013, 1, 1));
ediDocument.Segments[7].Element(04).RealValue.IsEqualTo(19.99m);
ediDocument.Segments[8].Element(01).NumericValue(0).IsEqualTo(1);
```

Component elements and element repetitions are supported and can be accessed by drilling further into the EdiElement object returned by ediDocument.Segments[...].Element(...).

Converting EDI to XML
---------------------

Converting EDI to XML requires two documents: the actual EDI data, as well as an XML mapping, which is essentially an EDI specification.

Given an EDI document 856.txt:

```
ISA*00*          *00*          *ZZ*SENDER         *ZZ*RECEIVER       *130101*0000*U*00401*000000001*0*P*>~
GS*SH*SENDER*RECEIVER*20130101*0000*1*X*004010~
ST*856*0001~
BSN*00*999999*20130101*0000*0001~
HL*1**S~
TD1*CTN*2****G*8.5*LB~
REF*BM*999999~
DTM*011*20130101~
N1*ST**92*9999~
HL*2*1*O~
PRF*Z9999999~
HL*3*2*P~
MAN*GM*00009999990000000019~
HL*4*3*I~
LIN**UP*999999999993~
SN1**3*EA~
HL*5*2*P~
MAN*GM*00009999990000000026~
HL*6*5*I~
LIN**UP*999999999993~
SN1**3*EA~
CTT*6~
SE*21*0001~
GE*1*1~
IEA*1*000000001~
```

The following XML mapping can be written up to indicate how to process EDI data. A detailed guide to the XML mapping format can be found further in this document.

Consider that we have the XML mapping in 856mapping.xml:

```xml
<mapping>
    <st/>
    <bsn>
        <bsn01>
            <option definition="original">00</option>
            <option definition="duplicate">07</option>
        </bsn01>
        <bsn03 type="dt"/>
        <bsn04 type="tm"/>
    </bsn>
    <hlloop>
        <hl>
            <hl03 restrict="true">
                <option>s</option>
            </hl03>
        </hl>
        <td1>
            <td102 type="n0"/>
            <td107 type="r"/>
        </td1>
        <ref/>
        <dtm>
            <dtm02 type="dt"/>
        </dtm>
        <n1loop>
            <n1></n1>
        </n1loop>
    </hlloop>
    <hlloop>
        <hl>
            <hl03 restrict="true">
                <option>o</option>
            </hl03>
        </hl>
        <prf/>
    </hlloop>
    <hlloop>
        <hl>
            <hl03 restrict="true">
                <option>p</option>
            </hl03>
        </hl>
        <man/>
    </hlloop>
    <hlloop>
        <hl>
            <hl03 restrict="true">
                <option>i</option>
            </hl03>
        </hl>
        <lin/>
        <sn1>
            <sn102 type="r"/>
        </sn1>
    </hlloop>
    <ctt>
        <ctt01 type="n0"/>
    </ctt>
    <se>
        <se01 type="n0"/>
    </se>
</mapping>
```

We can convert the EDI into XML with the following code:

```csharp
EdiDocument ediDocument = EdiDocument.Load("856.txt");
ediDocument.TransactionSets.Count.IsEqualTo(1);
EdiTransactionSet transactionSet = ediDocument.TransactionSets[0];
EdiMapping ediMapping = EdiMapping.Load("856mapping.xml");
XDocument xml = ediMapping.Map(transactionSet.Segments);
```

The resulting XML will be:

```xml
<?xml version="1.0" encoding="utf-8"?>
<mapping>
  <st>
    <st01>856</st01>
    <st02>0001</st02>
  </st>
  <bsn>
    <bsn01 definition="original">00</bsn01>
    <bsn02>999999</bsn02>
    <bsn03 type="dt">2013-01-01</bsn03>
    <bsn04 type="tm">00:00</bsn04>
    <bsn05>0001</bsn05>
  </bsn>
  <hlloop>
    <hl>
      <hl01>1</hl01>
      <hl03>S</hl03>
    </hl>
    <td1>
      <td101>CTN</td101>
      <td102 type="n0">2</td102>
      <td106>G</td106>
      <td107 type="r">8.5</td107>
      <td108>LB</td108>
    </td1>
    <ref>
      <ref01>BM</ref01>
      <ref02>999999</ref02>
    </ref>
    <dtm>
      <dtm01>011</dtm01>
      <dtm02 type="dt">2013-01-01</dtm02>
    </dtm>
    <n1loop>
      <n1>
        <n101>ST</n101>
        <n103>92</n103>
        <n104>9999</n104>
      </n1>
    </n1loop>
  </hlloop>
  <hlloop>
    <hl>
      <hl01>2</hl01>
      <hl02>1</hl02>
      <hl03>O</hl03>
    </hl>
    <prf>
      <prf01>Z9999999</prf01>
    </prf>
  </hlloop>
  <hlloop>
    <hl>
      <hl01>3</hl01>
      <hl02>2</hl02>
      <hl03>P</hl03>
    </hl>
    <man>
      <man01>GM</man01>
      <man02>00009999990000000019</man02>
    </man>
  </hlloop>
  <hlloop>
    <hl>
      <hl01>4</hl01>
      <hl02>3</hl02>
      <hl03>I</hl03>
    </hl>
    <lin>
      <lin02>UP</lin02>
      <lin03>999999999993</lin03>
    </lin>
    <sn1>
      <sn102 type="r">3</sn102>
      <sn103>EA</sn103>
    </sn1>
  </hlloop>
  <hlloop>
    <hl>
      <hl01>5</hl01>
      <hl02>2</hl02>
      <hl03>P</hl03>
    </hl>
    <man>
      <man01>GM</man01>
      <man02>00009999990000000026</man02>
    </man>
  </hlloop>
  <hlloop>
    <hl>
      <hl01>6</hl01>
      <hl02>5</hl02>
      <hl03>I</hl03>
    </hl>
    <lin>
      <lin02>UP</lin02>
      <lin03>999999999993</lin03>
    </lin>
    <sn1>
      <sn102 type="r">3</sn102>
      <sn103>EA</sn103>
    </sn1>
  </hlloop>
  <ctt>
    <ctt01 type="n0">6</ctt01>
  </ctt>
  <se>
    <se01 type="n0">21</se01>
    <se02>0001</se02>
  </se>
</mapping>
```

Supposing we save that XML into a file 856.xml, we can load the EDI data back into an EdiDocument with:

```csharp
EdiDocument.LoadXml("856.xml");
```

Writing out EDI
---------------

This code:

```csharp
var ediDocument = new EdiDocument();

var isa = new EdiSegment("ISA");
isa[01] = "00";
isa[02] = "".PadRight(10);
isa[03] = "00";
isa[04] = "".PadRight(10);
isa[05] = "ZZ";
isa[06] = "SENDER".PadRight(15);
isa[07] = "ZZ";
isa[08] = "RECEIVER".PadRight(15);
isa[09] = EdiValue.Date(6, DateTime.Now);
isa[10] = EdiValue.Time(4, DateTime.Now);
isa[11] = "U";
isa[12] = "00400";
isa[13] = 1.ToString("d9");
isa[14] = "0";
isa[15] = "P";
isa[16] = ">";
ediDocument.Segments.Add(isa);

var gs = new EdiSegment("GS");
gs[01] = "PO";
gs[02] = "SENDER";
gs[03] = "RECEIVER";
gs[04] = EdiValue.Date(8, DateTime.Now);
gs[05] = EdiValue.Time(4, DateTime.Now);
gs[06] = EdiValue.Numeric(0, 1);
gs[07] = "X";
gs[08] = "004010";
ediDocument.Segments.Add(gs);

// more segments...

ediDocument.Options.SegmentTerminator = '~';
ediDocument.Options.ElementSeparator = '*';
ediDocument.Save("save.txt");
```

Will produce the following file:

```
ISA*00*          *00*          *ZZ*SENDER         *ZZ*RECEIVER       *130305*1618*U*00400*000000001*0*P*>~
GS*PO*SENDER*RECEIVER*20130305*1618*1*X*004010~
...
```

Creating an XML mapping
-----------------------

The XML mapping is used to convert EDI, which in itself does not specify hierarchical structure, nor data types, to an XML document which has both of these things.

The XML mapping is essentially an EDI specification in an XML format. It includes declaration of what segments will be in the EDI, how they are arranged in loops, data types of elements, and a few more details.

An XML mapping starts with a root element, which can be named anything:

```xml
<mapping/>
```

What goes inside the root will be a collection of segments and loops that are expected to be found in the EDI data. For example, if we're expecting to encounter an ST segment, we would add that inside the root:

```xml
<mapping>
    <st/>
</mapping>
```

When EDI data is processed together with this XML mapping, it will fill the XML mapping with values. For example:

```xml
<mapping>
    <st>
        <st01>856</st01>
        <st02>0001</st02>
    </st>
</mapping>
```

Note that any elements or segments not in the mapping will be added to the resulting XML automatically, and anything in the mapping not found in the actual EDI data will simply be skipped.

If we expect a BSN segment, we can map it like so:

```xml
<bsn>
    <bsn01>
        <option definition="original">00</option>
        <option definition="duplicate">07</option>
    </bsn01>
    <bsn03 type="dt"/>
    <bsn04 type="tm"/>
</bsn>
```

Now, if we encounter a BSN segment of the form:

```
BSN*00*999999*20130101*0000*0001~
```

We can expect to see the mapping XML filled as such:

```xml
<bsn>
    <bsn01 definition="original">00</bsn01>
    <bsn02>999999</bsn02>
    <bsn03 type="dt">2013-01-01</bsn03>
    <bsn04 type="tm">00:00</bsn04>
</bsn>
```

Notice that bsn01 contains a definition attribute, where it records the definition of the code 00 if there were <option> elements specified in the mapping.

Also note that by specifying a type on elements bsn03 and bsn04, their raw EDI values have been converted to forms more conveniently parsed by .NET methods. That is to say, types dt (date) and tm (time) produce values that can be passed to DateTime.Parse(). Types n# and r can be passed to decimal.Parse().

A loop can be indicated by adding an element with "loop" at the end of its name:

```xml
<n1loop/>
```

Inside the loop can be more segments and inner loops. Say that we have the following mapping for an N1 loop:

```xml
<n1loop>
    <n1/>
    <n3/>
    <n4/>
</n1loop>
```

If we use that mapping to process this EDI data:

```
N1*SF*ORIGIN COMPANY~
N3*123 START ST~
N4*ANYTOWN*OR*45678~
N1*ST*DESTINATION COMPANY~
N3*789 END ST~
N4*ANYTOWN*DE*23456~
```

We will have:

```xml
<n1loop>
    <n1>
        <n101>SF</n101>
        <n102>ORIGIN COMPANY</n102>
    </n1>
    <n3>
        <n301>123 START ST</n301>
    </n3>
    <n4>
        <n401>ANYTOWN</n401>
        <n402>OR</n402>
        <n403>45678</n403>
    </n4>
</n1loop>
<n1loop>
    <n1>
        <n101>ST</n101>
        <n102>DESTINATION COMPANY</n102>
    </n1>
    <n3>
        <n301>789 END ST</n301>
    </n3>
    <n4>
        <n401>ANYTOWN</n401>
        <n402>DE</n402>
        <n403>23456</n403>
    </n4>
</n1loop>
```

A loop is created in the resulting XML when the first segment in the loop is encountered in the EDI data. In the previous mapping, EdiMapper will start a new n1loop when an N1 segment is encountered in the EDI. The loop will not close until the first segment is reencountered, where a new loop of the same kind will be created, or a segment defined at a higher level in the mapping is found, where the current loop will be exited.

Take the following XML mapping for instance:

```xml
<mapping>
    <st/>
    <bsn>
        <bsn01>
            <option definition="original">00</option>
            <option definition="duplicate">07</option>
        </bsn01>
        <bsn03 type="dt"/>
        <bsn04 type="tm"/>
    </bsn>
    <hlloop>
        <hl>
            <hl03 restrict="true">
                <option>s</option>
            </hl03>
        </hl>
        <td1>
            <td102 type="n0"/>
            <td107 type="r"/>
        </td1>
        <ref/>
        <dtm>
            <dtm02 type="dt"/>
        </dtm>
        <n1loop>
            <n1></n1>
        </n1loop>
    </hlloop>
    <hlloop>
        <hl>
            <hl03 restrict="true">
                <option>o</option>
            </hl03>
        </hl>
        <prf/>
    </hlloop>
    <hlloop>
        <hl>
            <hl03 restrict="true">
                <option>p</option>
            </hl03>
        </hl>
        <man/>
    </hlloop>
    <hlloop>
        <hl>
            <hl03 restrict="true">
                <option>i</option>
            </hl03>
        </hl>
        <lin/>
        <sn1>
            <sn102 type="r"/>
        </sn1>
    </hlloop>
    <ctt>
        <ctt01 type="n0"/>
    </ctt>
    <se>
        <se01 type="n0"/>
    </se>
</mapping>
```

When an HL segment is encountered, an hlloop is started. Subsequent segments will continue to be placed in this loop until:

* Another HL segment is encountered, where the current hlloop will end and another hlloop will be started, or
* A CTT or SE segment is encountered, which are indicated in the XML mapping to be outside the hlloop, causing the hlloop to end.

Note that in some cases, such as for the 856, there are multiple loops with the same first segment ID per the EDI specification, but containing different sets of expected segments depending on a value in that first segment. In the case of the 856, there are multiple HL loops, where HL03 indicates the hierarchical level of the loop and the sort of segments expected in the loop.

Taking a look at the above XML mapping, multiple loops with the same starting segment can be specified. The way to direct the EdiMapper to enter the correct loop is to indicate that an element in the first segment must be a specific value.

In the first <hlloop>, <hl03> is restricted to its options, only one being provided. This signifies that this loop will only be entered if the HL segment encountered in the actual EDI data has S in HL03.

Though not commonly used, component elements are also supported in the XML mapping. Simply define elements in elements:

```xml
<ak4>
    <ak401>
        <c03001 type="n0"/>
        <c03002 type="n0"/>
        <c03003 type="n0"/>
    </ak401>
    <ak402 type="n0"/>
    <ak403 type="id"/>
    <ak404 type="an"/>
</ak4>
```

Element repetitions are also supported. If an element is repeated, it will appear in the resulting XML multiple times.
