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
        /// <param name="isa">The ISA segment associated with this transaction set.</param>
        /// <param name="gs">The GS segment associated with this transaction set.</param>
        public EdiTransactionSet(EdiSegment isa, EdiSegment gs)
        {
            Isa = isa;
            Gs = gs;
            Segments = new List<EdiSegment>();
        }

        /// <summary>
        /// Gets the ISA segment associated with this transaction set.
        /// </summary>
        public EdiSegment Isa { get; private set; }

        /// <summary>
        /// Gets the GS segment associated with this transaction set.
        /// </summary>
        public EdiSegment Gs { get; private set; }

        /// <summary>
        /// Gets a list of segments belonging to this transaction set.
        /// </summary>
        public IList<EdiSegment> Segments { get; private set; }
    }
}