using System.Collections.Generic;
using System.Text;

namespace EdiTools
{
    /// <summary>
    /// Represents an EDI element.
    /// </summary>
    public class EdiElement : EdiValue
    {
        /// <summary>
        /// Initializes a new instance of the EdiElement class.
        /// </summary>
        public EdiElement()
        {
            Repetitions = new List<EdiRepetition>();
        }

        /// <summary>
        /// Initializes a new instance of the EdiElement class with the specified value.
        /// </summary>
        /// <param name="value">The initial value of the element.</param>
        public EdiElement(string value)
        {
            Repetitions = new List<EdiRepetition> {new EdiRepetition(value)};
        }

        /// <summary>
        /// Gets a list of the repetitions in this element.
        /// </summary>
        public IList<EdiRepetition> Repetitions { get; private set; }

        /// <summary>
        /// Gets the value of the first repetition or sets the value of the whole element.
        /// </summary>
        public override string Value
        {
            get { return Repetitions[0].Value; }

            set
            {
                Repetitions.Clear();
                Repetitions.Add(new EdiRepetition(value));
            }
        }

        /// <summary>
        /// Gets a list of the component elements in the first repetition of this element.
        /// </summary>
        public IList<EdiComponent> Components
        {
            get
            {
                if (Repetitions.Count == 0)
                    Repetitions.Add(new EdiRepetition());
                return Repetitions[0].Components;
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
        /// Returns the EDI for this element.
        /// </summary>
        /// <returns>A string containing the EDI.</returns>
        public override string ToString()
        {
            return ToString(null);
        }

        /// <summary>
        /// Returns the EDI for this element, optionally specifying separator characters.
        /// </summary>
        /// <param name="options">An EdiOptions that specifies separator characters.</param>
        /// <returns>A string containing the EDI.</returns>
        public string ToString(EdiOptions options)
        {
            var edi = new StringBuilder();
            for (int i = 0; i < Repetitions.Count; i++)
            {
                if (i > 0)
                    edi.Append(options != null && options.RepetitionSeparator.HasValue ? options.RepetitionSeparator.Value : EdiOptions.DefaultRepetitionSeparator);
                edi.Append(Repetitions[i].ToString(options));
            }
            return edi.ToString();
        }
    }
}