using System.Collections.Generic;

namespace EdiTools
{
    /// <summary>
    /// Represents an EDI transaction set.
    /// </summary>
    public class EdiTransactionSet
    {
        /// <summary>
        /// Initializes a new instance of the EdiTransactionSet class.
        /// </summary>
        /// <param name="interchangeHeader">The interchange header segment associated with this transaction set.</param>
        /// <param name="functionalGroupHeader">The functional group header segment associated with this transaction set.</param>
        public EdiTransactionSet(EdiSegment interchangeHeader, EdiSegment functionalGroupHeader)
        {
            InterchangeHeader = interchangeHeader;
            FunctionalGroupHeader = functionalGroupHeader;
            Segments = new List<EdiSegment>();
        }

        /// <summary>
        /// Gets the interchange header segment associated with this transaction set.
        /// </summary>
        public EdiSegment InterchangeHeader { get; private set; }

        /// <summary>
        /// Gets the functional group header segment associated with this transaction set.
        /// </summary>
        public EdiSegment FunctionalGroupHeader { get; private set; }

        /// <summary>
        /// Gets a list of segments belonging to this transaction set.
        /// </summary>
        public IList<EdiSegment> Segments { get; private set; }
    }
}