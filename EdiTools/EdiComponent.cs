using System;

namespace EdiTools
{
    /// <summary>
    /// Represents an EDI component element.
    /// </summary>
    public class EdiComponent : EdiValue
    {
        private string _value;

        /// <summary>
        /// Initializes a new instance of the EdiComponent class with the specified value.
        /// </summary>
        /// <param name="value">The initial value of the component element.</param>
        public EdiComponent(string value)
        {
            _value = value;
        }

        /// <summary>
        /// Gets or sets the value of the component element.
        /// </summary>
        public override string Value
        {
            get { return _value; }
            set { _value = value; }
        }

        /// <summary>
        /// Returns the EDI for this component element.
        /// </summary>
        /// <returns>A string containing the EDI.</returns>
        public override string ToString()
        {
            return ToString(null);
        }

        /// <summary>
        /// Returns the EDI for this component element, optionally specifying separator characters.
        /// </summary>
        /// <param name="options">An EdiOptions that specifies separator characters.</param>
        /// <returns>A string containing the EDI.</returns>
        public string ToString(EdiOptions options)
        {
            if (_value.IndexOf(options != null && options.SegmentTerminator.HasValue ? options.SegmentTerminator.Value : EdiOptions.DefaultSegmentTerminator) != -1)
                throw new FormatException(string.Format("'{0}' contains the segment terminator.", _value));
            if (_value.IndexOf(options != null && options.ElementSeparator.HasValue ? options.ElementSeparator.Value : EdiOptions.DefaultElementSeparator) != -1)
                throw new FormatException(string.Format("'{0}' contains the element separator.", _value));
            if (options != null && options.RepetitionSeparator.HasValue && _value.IndexOf(options.RepetitionSeparator.Value) != -1)
                throw new FormatException(string.Format("'{0}' contains the repetition separator.", _value));
            if (_value.IndexOf(options != null && options.ComponentSeparator.HasValue ? options.ComponentSeparator.Value : EdiOptions.DefaultComponentSeparator) != -1)
                throw new FormatException(string.Format("'{0}' contains the component separator.", _value));
            return _value;
        }
    }
}