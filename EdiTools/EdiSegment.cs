using System;
using System.Collections.Generic;
using System.Text;

namespace EdiTools
{
    /// <summary>
    /// Represents an EDI segment.
    /// </summary>
    public class EdiSegment
    {
        /// <summary>
        /// Initializes a new instance of the EdiSegment class with the specified ID.
        /// </summary>
        /// <param name="id">The ID of this segment, such as "ISA".</param>
        public EdiSegment(string id = null)
        {
            Id = id;
            Elements = new List<EdiElement>();
        }

        /// <summary>
        /// Gets or sets the ID of this segment.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets a list of the elements in this segment.
        /// </summary>
        public IList<EdiElement> Elements { get; private set; }

        /// <summary>
        /// Gets or sets the value of the element at the specified position.
        /// </summary>
        /// <param name="position">The position of the element, starting at 1.</param>
        /// <returns>A string containing the value of the element.</returns>
        public string this[int position]
        {
            get
            {
                int index = position - 1;
                if (Elements.Count <= index || Elements[index] == null)
                    return null;
                return Elements[index].Value;
            }

            set
            {
                int index = position - 1;
                if (!string.IsNullOrEmpty(value))
                {
                    while (Elements.Count <= index)
                        Elements.Add(null);
                    Elements[index] = new EdiElement(value);
                }
                else if (Elements.Count > index)
                    Elements[index] = null;
            }
        }

        /// <summary>
        /// Returns the element at the specified position.
        /// </summary>
        /// <param name="position">The position of the element, starting at 1.</param>
        /// <returns>An EdiElement representing the element.</returns>
        public EdiElement Element(int position)
        {
            int index = position - 1;
            return Elements.Count <= index ? null : Elements[index];
        }

        /// <summary>
        /// Places an EdiElement at the specified position.
        /// </summary>
        /// <param name="position">The position of the element, starting at 1.</param>
        /// <param name="element">An EdiElement to place in this segment.</param>
        public void Element(int position, EdiElement element)
        {
            int index = position - 1;
            if (element != null)
            {
                while (Elements.Count <= index)
                    Elements.Add(null);
                Elements[index] = element;
            }
            else if (Elements.Count > index)
                Elements[index] = null;
        }

        /// <summary>
        /// Returns the EDI for this segment.
        /// </summary>
        /// <returns>A string containing the EDI.</returns>
        public override string ToString()
        {
            return ToString(null);
        }

        /// <summary>
        /// Returns the EDI for this segment, optionally specifying separator characters.
        /// </summary>
        /// <param name="options">An EdiOptions that specifies separator characters.</param>
        /// <returns>A string containing the EDI.</returns>
        public string ToString(EdiOptions options)
        {
            var edi = new StringBuilder(Id);
            int lastElementIndex = GetLastElementIndex();
            for (int i = 0; i <= lastElementIndex; i++)
            {
                if (Id.Equals("UNA", StringComparison.OrdinalIgnoreCase))
                {
                    edi.Append(Elements[i].Value);
                    continue;
                }

                edi.Append(options != null && options.ElementSeparator.HasValue ? options.ElementSeparator : EdiOptions.DefaultElementSeparator);
                if (Elements[i] == null)
                    continue;
                if (Id.Equals("ISA", StringComparison.OrdinalIgnoreCase) &&
                    Elements[i].Value.Length == 1 &&
                    (i == 15 && Elements[i].Value[0] == (options != null && options.ComponentSeparator.HasValue ? options.ComponentSeparator.Value : EdiOptions.DefaultComponentSeparator) ||
                     i == 10 && options != null && Elements[i].Value[0] == options.RepetitionSeparator) &&
                    Elements[i].Repetitions.Count == 1 &&
                    Elements[i].Components.Count == 1)
                {
                    edi.Append(Elements[i].Value);
                }
                else
                    edi.Append(Elements[i].ToString(options));
            }
            edi.Append(options != null && options.SegmentTerminator.HasValue ? options.SegmentTerminator : EdiOptions.DefaultSegmentTerminator);
            return edi.ToString();
        }

        private int GetLastElementIndex()
        {
            for (int i = Elements.Count - 1; i >= 0; i--)
            {
                if (Elements[i] != null)
                    return i;
            }
            return -1;
        }
    }
}