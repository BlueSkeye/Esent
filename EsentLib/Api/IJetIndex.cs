using System;
using System.Collections.Generic;

namespace EsentLib.Api
{
    /// <summary></summary>
    [CLSCompliant(false)]
    public interface IJetIndex
    {
        /// <summary></summary>
        List<IJetIndexColumn> Columns { get; }

        /// <summary></summary>
        uint EntriesCount { get; }

        /// <summary></summary>
        uint KeyCount { get; }

        /// <summary></summary>
        string Name { get; }

        /// <summary></summary>
        IJetTable Owner { get; }
    }
}
