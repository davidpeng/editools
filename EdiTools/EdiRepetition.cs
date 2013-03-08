using System.Collections.Generic;
using System.Text;

namespace EdiTools
{
    /// <summary>
    /// Represents an EDI element repetition.
    /// </summary>
    public class EdiRepetition : EdiValue
    {
        /// <summary>
        /// Initializes a new instance of the EdiRepetition class.
        /// </summary>
        public EdiRepetition()
        {
            Components = new List<EdiComponent>();
        }

        /// <summary>
        /// Initializes a new instance of the EdiRepetition class with the specified value.
        /// </summary>
        /// <param name="value">The initial value of the element repetition.</param>
        public EdiRepetition(string value)
        {
            Components = new List<EdiComponent> {new EdiComponent(value)};
        }

        /// <summary>
        /// Gets a list of the component elements in this element repetition.
        /// </summary>
        public IList<EdiComponent> Components { get; private set; }

        /// <summary>
        /// Gets the value of the first component element or sets the value of the whole element repetition.
        /// </summary>
        public override string Value
        {
            get { return Components[0].Value; }

            set
            {
                Components.Clear();
                Components.Add(new EdiComponent(value));
            }
        }

        /// <summary>
        /// Gets or sets the value of the component element at the specified position.
        /// </summary>
        /// <param name="position">The position of the component element, starting at 1.</param>
        /// <returns>A string containing the value of the component element.</returns>
        public string this[int position]
        {
            get
            {
                int index = position - 1;
                if (Components.Count <= index || Components[index] == null)
                    return null;
                return Components[index].Value;
            }

            set
            {
                int index = position - 1;
                if (!string.IsNullOrEmpty(value))
                {
                    while (Components.Count <= index)
                        Components.Add(null);
                    Components[index] = new EdiComponent(value);
                }
                else if (Components.Count > index)
                    Components[index] = null;
            }
        }

        /// <summary>
        /// Returns the component element at the specified position.
        /// </summary>
        /// <param name="position">The position of the component element, starting at 1.</param>
        /// <returns>An EdiComponent representing the component element.</returns>
        public EdiComponent Component(int position)
        {
            int index = position - 1;
            return Components.Count <= index ? null : Components[index];
        }

        /// <summary>
        /// Returns the EDI for this element repetition.
        /// </summary>
        /// <returns>A string containing the EDI.</returns>
        public override string ToString()
        {
            return ToString(null);
        }

        /// <summary>
        /// Returns the EDI for this element repetition, optionally specifying separator characters.
        /// </summary>
        /// <param name="options">An EdiOptions that specifies separator characters.</param>
        /// <returns>A string containing the EDI.</returns>
        public string ToString(EdiOptions options)
        {
            var edi = new StringBuilder();
            int lastComponentIndex = GetLastComponentIndex();
            for (int i = 0; i <= lastComponentIndex; i++)
            {
                if (i > 0)
                    edi.Append(options != null && options.ComponentSeparator.HasValue
                                   ? options.ComponentSeparator.Value
                                   : EdiOptions.DefaultComponentSeparator);
                if (Components[i] == null)
                    continue;
                edi.Append(Components[i].ToString(options));
            }
            return edi.ToString();
        }

        private int GetLastComponentIndex()
        {
            for (int i = Components.Count - 1; i >= 0; i--)
            {
                if (Components[i] != null)
                    return i;
            }
            return -1;
        }
    }
}