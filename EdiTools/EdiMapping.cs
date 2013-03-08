using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace EdiTools
{
    /// <summary>
    /// Represents an EDI to XML mapping.
    /// </summary>
    public class EdiMapping
    {
        private readonly Loop _root;

        private EdiMapping(XDocument xml)
        {
            if (xml.Root == null)
                throw new Exception("XML is missing a root element.");
            Errors = new List<string>();
            _root = ReadLoop(xml.Root);
        }

        /// <summary>
        /// Gets a list of errors encountered while loading or using this EdiMapping.
        /// </summary>
        public IList<string> Errors { get; private set; }

        /// <summary>
        /// Creates a new EdiMapping from a string.
        /// </summary>
        /// <param name="text">A string that contains an EDI to XML mapping.</param>
        /// <returns>An EdiMapping populated from the string that contains the EDI to XML mapping.</returns>
        public static EdiMapping Parse(string text)
        {
            XDocument xml = XDocument.Parse(text);
            return new EdiMapping(xml);
        }

        /// <summary>
        /// Creates a new EdiMapping from an XDocument.
        /// </summary>
        /// <param name="xml">An XDocument that contains an EDI to XML mapping.</param>
        /// <returns>An EdiMapping populated from the XDocument that contains the EDI to XML mapping.</returns>
        public static EdiMapping Load(XDocument xml)
        {
            return new EdiMapping(xml);
        }

        /// <summary>
        /// Creates a new EdiMapping from a file.
        /// </summary>
        /// <param name="fileName">A file name that references the file to load into a new EdiMapping.</param>
        /// <returns>An EdiMapping that contains the contents of the specified file.</returns>
        public static EdiMapping Load(string fileName)
        {
            XDocument xml = XDocument.Load(fileName);
            return new EdiMapping(xml);
        }

        /// <summary>
        /// Creates a new EdiMapping from a TextReader.
        /// </summary>
        /// <param name="reader">A TextReader that contains the content for the EdiMapping.</param>
        /// <returns>An EdiMapping that contains the contents of the specified TextReader.</returns>
        public static EdiMapping Load(TextReader reader)
        {
            XDocument xml = XDocument.Load(reader);
            return new EdiMapping(xml);
        }

        /// <summary>
        /// Creates a new EdiMapping instance by using the specified stream.
        /// </summary>
        /// <param name="stream">The stream that contains the EDI to XML mapping.</param>
        /// <returns>An EdiMapping object that reads the EDI to XML mapping that is contained in the stream.</returns>
        public static EdiMapping Load(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                XDocument xml = XDocument.Load(reader);
                return new EdiMapping(xml);
            }
        }

        private Node ReadNode(XElement xml)
        {
            if (xml.Name.LocalName.EndsWith("loop"))
                return ReadLoop(xml);
            return ReadSegment(xml);
        }

        private Loop ReadLoop(XElement xml)
        {
            var loop = new Loop(xml.Name.LocalName);
            foreach (XElement element in xml.Elements())
                loop.Nodes.Add(ReadNode(element));
            return loop;
        }

        private Segment ReadSegment(XElement xml)
        {
            var segment = new Segment(xml.Name.LocalName);
            foreach (XElement element in xml.Elements())
            {
                int elementIndex = GetElementIndex(element.Name.LocalName);
                while (segment.Elements.Count <= elementIndex)
                    segment.Elements.Add(null);
                if (segment.Elements[elementIndex] != null)
                {
                    Errors.Add(
                        string.Format(
                            "Element '{0}' occupies a position in the segment already taken by element '{1}'.",
                            element.Name.LocalName, segment.Elements[elementIndex].Id));
                    continue;
                }
                segment.Elements[elementIndex] = ReadElement(element);
            }
            return segment;
        }

        private int GetElementIndex(string elementId)
        {
            int position;
            if (elementId.Length < 2 || !int.TryParse(elementId.Substring(elementId.Length - 2), out position))
            {
                Errors.Add(string.Format("Element '{0}' does not have a valid segment position.", elementId));
                return -1;
            }
            return position - 1;
        }

        private Element ReadElement(XElement xml)
        {
            string type = GetElementType(xml);
            bool restrict = GetElementRestrict(xml);
            var element = new Element(xml.Name.LocalName, type, restrict);
            foreach (XElement xmlElement in xml.Elements())
            {
                if (xmlElement.Name.LocalName == "option")
                {
                    string option = xmlElement.Value;
                    if (element.Options.Contains(option))
                    {
                        Errors.Add(string.Format("Option '{0}' is already defined in the element.", option));
                        continue;
                    }
                    element.Options[option] = GetOptionDefinition(xmlElement);
                    continue;
                }

                int componentIndex = GetElementIndex(xmlElement.Name.LocalName);
                while (element.Components.Count <= componentIndex)
                    element.Components.Add(null);
                if (element.Components[componentIndex] != null)
                {
                    Errors.Add(
                        string.Format(
                            "Component '{0}' occupies a position in the element already taken by component '{1}'.",
                            xmlElement.Name.LocalName, element.Components[componentIndex].Id));
                    continue;
                }
                element.Components[componentIndex] = ReadComponent(xmlElement);
            }
            return element;
        }

        private string GetElementType(XElement xml)
        {
            XAttribute typeAttribute = xml.Attribute("type");
            if (typeAttribute == null)
                return null;
            if (IsValidElementType(typeAttribute.Value))
                return typeAttribute.Value;
            Errors.Add(string.Format("'{0}' is not a valid type.", typeAttribute.Value));
            return null;
        }

        private bool IsValidElementType(string type)
        {
            return Regex.IsMatch(type, "^id|an|dt|tm|n[0-9]|r$");
        }

        private bool GetElementRestrict(XElement xml)
        {
            XAttribute restrictAttribute = xml.Attribute("restrict");
            if (restrictAttribute == null)
                return false;
            return restrictAttribute.Value == "true";
        }

        private string GetOptionDefinition(XElement xml)
        {
            XAttribute definitionAttribute = xml.Attribute("definition");
            if (definitionAttribute == null)
                return null;
            return definitionAttribute.Value;
        }

        private Component ReadComponent(XElement xml)
        {
            string type = GetElementType(xml);
            bool restrict = GetElementRestrict(xml);
            var component = new Component(xml.Name.LocalName, type, restrict);
            foreach (XElement element in xml.Elements())
            {
                if (element.Name.LocalName != "option")
                    continue;
                string option = element.Value;
                if (component.Options.Contains(option))
                {
                    Errors.Add(string.Format("Option '{0}' is already defined in the component.", option));
                    continue;
                }
                component.Options[option] = GetOptionDefinition(element);
            }
            return component;
        }

        /// <summary>
        /// Converts the specified list of EdiSegments into an XDocument using this EdiMapping.
        /// </summary>
        /// <param name="segments">A list of EdiSegments to convert into an XDocument.</param>
        /// <returns>An XDocument containing the XML representations of the EdiSegments.</returns>
        public XDocument Map(IList<EdiSegment> segments)
        {
            var mapState = new MapState(segments);
            XElement rootElement = Map(mapState, _root);
            return new XDocument(rootElement);
        }

        private XElement Map(MapState mapState, Loop loop)
        {
            var xml = new XElement(loop.Id);
            string previousSegmentId = null;
            var loopState = new MapState.LoopState(loop);
            mapState.LoopStates.Push(loopState);
            while (mapState.SegmentIndex < mapState.Segments.Count)
            {
                EdiSegment segment = mapState.Segments[mapState.SegmentIndex];
                if (loopState.VisitedSegmentIds.Contains(segment.Id))
                    break;
                Node mapping = loop.FindMatchingNode(segment);
                if (mapping == null && SegmentWasUnvisitedInOuterLoops(segment, mapState))
                    break;
                if (mapping is Loop)
                    xml.Add(Map(mapState, (Loop) mapping));
                else
                {
                    xml.Add(MapSegment(segment, (Segment) mapping));
                    mapState.SegmentIndex++;
                }
                if (previousSegmentId == null)
                {
                    if (loop != _root)
                        loopState.VisitedSegmentIds.Add(segment.Id);
                }
                else if (previousSegmentId != segment.Id)
                    loopState.VisitedSegmentIds.Add(previousSegmentId);
                previousSegmentId = segment.Id;
            }
            mapState.LoopStates.Pop();
            return xml;
        }

        private bool SegmentWasUnvisitedInOuterLoops(EdiSegment segment, MapState mapState)
        {
            return
                mapState.LoopStates.Any(
                    state =>
                    !state.VisitedSegmentIds.Contains(segment.Id) && state.Loop.FindMatchingNode(segment) != null);
        }

        private XElement MapSegment(EdiSegment segment, Segment mapping)
        {
            var xml = new XElement(mapping != null ? mapping.Id : segment.Id);
            for (int i = 0; i < segment.Elements.Count; i++)
            {
                EdiElement element = segment.Elements[i];
                if (element == null)
                    continue;
                Element elementMapping = null;
                string segmentId;
                if (mapping != null)
                {
                    if (mapping.Elements.Count > i)
                        elementMapping = mapping.Elements[i];
                    segmentId = mapping.Id;
                }
                else
                    segmentId = segment.Id;
                string defaultElementId = segmentId + (i + 1).ToString("d2");
                xml.Add(MapElement(element, elementMapping, defaultElementId));
            }
            return xml;
        }

        private IEnumerable<XElement> MapElement(EdiElement element, Element mapping, string defaultElementId)
        {
            var repetitions = new List<XElement>();
            foreach (EdiRepetition repetition in element.Repetitions)
            {
                var xml = new XElement(mapping != null ? mapping.Id : defaultElementId);
                if (repetition.Components.Count == 1)
                {
                    if (mapping != null)
                    {
                        if (mapping.Components.Count == 0)
                        {
                            string mappedValue;
                            if (mapping.Type == null || !MapValue(repetition, mapping.Type, out mappedValue))
                                xml.Value = repetition.Value;
                            else
                            {
                                xml.SetAttributeValue("type", mapping.Type);
                                xml.Value = mappedValue;
                            }
                            if (mapping.Options.Contains(repetition.Value))
                            {
                                string definition = mapping.Options[repetition.Value];
                                if (definition != null && definition.Trim() != string.Empty)
                                    xml.SetAttributeValue("definition", definition);
                            }
                        }
                        else
                            xml.Add(MapComponent(repetition.Components[0], mapping.Components[0], mapping.Id + "01"));
                    }
                    else
                        xml.Value = repetition.Value;
                }
                else
                {
                    for (int i = 0; i < repetition.Components.Count; i++)
                    {
                        EdiComponent component = repetition.Components[i];
                        if (component == null)
                            continue;
                        Component componentMapping = null;
                        string elementId;
                        if (mapping != null)
                        {
                            if (mapping.Components.Count > i)
                                componentMapping = mapping.Components[i];
                            elementId = mapping.Id;
                        }
                        else
                            elementId = defaultElementId;
                        string defaultComponentId = elementId + (i + 1).ToString("d2");
                        xml.Add(MapComponent(component, componentMapping, defaultComponentId));
                    }
                }
                repetitions.Add(xml);
            }
            return repetitions;
        }

        private XElement MapComponent(EdiComponent component, Component mapping, string defaultComponentId)
        {
            if (mapping != null)
            {
                var xml = new XElement(mapping.Id);
                string mappedValue;
                if (mapping.Type == null || !MapValue(component, mapping.Type, out mappedValue))
                    xml.Value = component.Value;
                else
                {
                    xml.SetAttributeValue("type", mapping.Type);
                    xml.Value = mappedValue;
                }
                if (mapping.Options.Contains(component.Value))
                {
                    string definition = mapping.Options[component.Value];
                    xml.SetAttributeValue("definition", definition);
                }
                return xml;
            }
            return new XElement(defaultComponentId, component.Value);
        }

        private bool MapValue(EdiValue node, string type, out string mappedValue)
        {
            try
            {
                switch (type)
                {
                    case "id":
                    case "an":
                        mappedValue = node.Value;
                        return true;
                    case "dt":
                        mappedValue = node.IsoDate;
                        return true;
                    case "tm":
                        mappedValue = node.IsoTime;
                        return true;
                    case "r":
                        mappedValue = node.RealValue.ToString(CultureInfo.InvariantCulture);
                        return true;
                    default:
                        int decimals = int.Parse(type.Substring(1));
                        mappedValue = node.NumericValue(decimals).ToString(CultureInfo.InvariantCulture);
                        return true;
                }
            }
            catch (FormatException)
            {
                Errors.Add(string.Format("'{0}' is not a valid value of type '{1}'.", node.Value, type));
                mappedValue = null;
                return false;
            }
        }

        #region Nested type: Component

        private class Component
        {
            private readonly bool _restrict;

            public Component(string id, string type, bool restrict)
            {
                Id = id;
                Type = type;
                Options = new Options();
                _restrict = restrict;
            }

            public string Id { get; private set; }
            public string Type { get; private set; }
            public Options Options { get; private set; }

            public bool IsMatch(EdiComponent component)
            {
                return !_restrict || (component != null && Options.Contains(component.Value));
            }
        }

        #endregion

        #region Nested type: Element

        private class Element
        {
            private readonly bool _restrict;

            public Element(string id, string type, bool restrict)
            {
                Id = id;
                Type = type;
                Components = new List<Component>();
                Options = new Options();
                _restrict = restrict;
            }

            public string Id { get; private set; }
            public string Type { get; private set; }
            public IList<Component> Components { get; private set; }
            public Options Options { get; private set; }

            public bool IsMatch(EdiElement element)
            {
                if (_restrict && element == null)
                    return false;
                foreach (EdiRepetition repetition in element.Repetitions)
                {
                    if (_restrict && !Options.Contains(repetition.Value))
                        return false;
                    if (repetition.Components.Where((t, i) => Components.Count > i && Components[i] != null &&
                                                              !Components[i].IsMatch(t)).Any())
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        #endregion

        #region Nested type: Loop

        private class Loop : Node
        {
            public Loop(string id)
            {
                Id = id;
                Nodes = new List<Node>();
            }

            public IList<Node> Nodes { get; private set; }

            public Node FindMatchingNode(EdiSegment segment)
            {
                foreach (Node node in Nodes)
                {
                    if (node is Segment)
                    {
                        var segmentMapping = (Segment) node;
                        if (segmentMapping.IsMatch(segment))
                            return node;
                    }
                    else
                    {
                        var loop = (Loop) node;
                        Segment segmentMapping = loop.GetFirstSegment();
                        if (segmentMapping != null && segmentMapping.IsMatch(segment))
                            return node;
                    }
                }
                return null;
            }

            private Segment GetFirstSegment()
            {
                if (Nodes.Count == 0)
                    return null;
                var firstSegment = Nodes[0] as Segment;
                if (firstSegment != null)
                    return firstSegment;
                var loop = (Loop) Nodes[0];
                return loop.GetFirstSegment();
            }
        }

        #endregion

        #region Nested type: MapState

        private class MapState
        {
            public MapState(IList<EdiSegment> segments)
            {
                Segments = segments;
                LoopStates = new Stack<LoopState>();
            }

            public IList<EdiSegment> Segments { get; private set; }
            public int SegmentIndex { get; set; }
            public Stack<LoopState> LoopStates { get; private set; }

            #region Nested type: LoopState

            public class LoopState
            {
                public LoopState(Loop loop)
                {
                    Loop = loop;
                    VisitedSegmentIds = new HashSet<string>();
                }

                public Loop Loop { get; private set; }
                public HashSet<string> VisitedSegmentIds { get; private set; }
            }

            #endregion
        }

        #endregion

        #region Nested type: Node

        private abstract class Node
        {
            public string Id { get; protected set; }
        }

        #endregion

        #region Nested type: Options

        private class Options
        {
            private readonly IList<Option> _options = new List<Option>();

            public string this[string key]
            {
                get
                {
                    Option option = _options.FirstOrDefault(o => o.Value.Equals(key, StringComparison.OrdinalIgnoreCase));
                    if (option == null)
                        throw new KeyNotFoundException();
                    return option.Definition;
                }

                set
                {
                    Option option = _options.FirstOrDefault(o => o.Value.Equals(key, StringComparison.OrdinalIgnoreCase));
                    if (option != null)
                        throw new NotSupportedException();
                    _options.Add(new Option(key, value));
                }
            }

            public bool Contains(string key)
            {
                return _options.Any(option => option.Value.Equals(key, StringComparison.OrdinalIgnoreCase));
            }

            #region Nested type: Option

            private class Option
            {
                public Option(string value, string definition)
                {
                    Value = value;
                    Definition = definition;
                }

                public string Value { get; private set; }
                public string Definition { get; private set; }
            }

            #endregion
        }

        #endregion

        #region Nested type: Segment

        private class Segment : Node
        {
            public Segment(string id)
            {
                Id = id;
                Elements = new List<Element>();
            }

            public IList<Element> Elements { get; private set; }

            public bool IsMatch(EdiSegment segment)
            {
                return Id.Equals(segment.Id, StringComparison.OrdinalIgnoreCase) &&
                       !segment.Elements.Where(
                           (t, i) => Elements.Count > i && Elements[i] != null && !Elements[i].IsMatch(t)).Any();
            }
        }

        #endregion
    }
}