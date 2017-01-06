using System;

using EsentLib.Jet;

namespace EsentLib.Api
{
    /// <summary>A read-only definition of a column characteristics.</summary>
    [CLSCompliant(false)]
    public interface IJetColumn
    {
        /// <summary>Column charateristis.</summary>
        uint Characteristics { get; }

        /// <summary>Column code page.</summary>
        ushort CodePage { get; }

        /// <summary>Column default value.</summary>
        string DefaultValue { get; }

        /// <summary>Column id.</summary>
        JET_COLUMNID Id { get; }

        /// <summary>Column maximum length (unit unknown).</summary>
        int MaximumLength { get; }

        /// <summary>Column name.</summary>
        string Name { get; }

        /// <summary>Get table this oolumn belongs to.</summary>
        IJetTable Owner { get; }

        /// <summary>Column type.</summary>
        JET_coltyp Type { get; }
    }
}
