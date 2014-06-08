namespace EdiTools
{
    /// <summary>
    /// Stores the separator characters to use when loading or saving EDI documents.
    /// </summary>
    public class EdiOptions
    {
        /// <summary>
        /// The default segment terminator to use when saving EDI documents where one is not specified.
        /// </summary>
        public static char DefaultSegmentTerminator = '\r';

        /// <summary>
        /// The default element separator to use when saving EDI documents where one is not specified.
        /// </summary>
        public static char DefaultElementSeparator = '*';

        /// <summary>
        /// The default component element separator to use when saving EDI documents where one is not specified.
        /// </summary>
        public static char DefaultComponentSeparator = '>';

        /// <summary>
        /// The default repetition separator to use when saving EDI documents where one is not specified.
        /// </summary>
        public static char DefaultRepetitionSeparator = '^';

        /// <summary>
        /// Initializes a new instance of the EdiOptions class.
        /// </summary>
        public EdiOptions()
        {
        }

        /// <summary>
        /// Initializes a new instance of the EdiOptions class with values copied from the specified EdiOptions parameter.
        /// </summary>
        /// <param name="source">An EdiOptions containing values to copy.</param>
        public EdiOptions(EdiOptions source)
        {
            SegmentTerminator = source.SegmentTerminator;
            ElementSeparator = source.ElementSeparator;
            ComponentSeparator = source.ComponentSeparator;
            RepetitionSeparator = source.RepetitionSeparator;
            DecimalIndicator = source.DecimalIndicator;
            ReleaseCharacter = source.ReleaseCharacter;
        }

        /// <summary>
        /// Gets or sets the segment terminator.
        /// </summary>
        public char? SegmentTerminator { get; set; }

        /// <summary>
        /// Gets or sets the element separator;
        /// </summary>
        public char? ElementSeparator { get; set; }

        /// <summary>
        /// Gets or sets the component element separator.
        /// </summary>
        public char? ComponentSeparator { get; set; }

        /// <summary>
        /// Gets or sets the repetition separator.
        /// </summary>
        public char? RepetitionSeparator { get; set; }

        /// <summary>
        /// Gets or sets the decimal indicator.
        /// </summary>
        public char? DecimalIndicator { get; set; }

        /// <summary>
        /// Gets or sets the release character.
        /// </summary>
        public char? ReleaseCharacter { get; set; }
    }
}